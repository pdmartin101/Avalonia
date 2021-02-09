﻿using System.Collections;
using System.Collections.Specialized;
using System.Reactive.Linq;
using Avalonia.Collections;
using Avalonia.Controls.Utils;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Presenters
{
    /// <summary>
    /// Represents an item virtualizer for an <see cref="ItemsPresenter"/> that doesn't actually
    /// virtualize items - it just creates a container for every item.
    /// </summary>
    internal class ItemVirtualizerLogical : ItemVirtualizer
    {
        private RealizedItems _realizedChildren;
        public int Id;
        private bool _estimated;
        private Size _estimatedSize;
        private double _viewport;
        private ScrollContentPresenter _scrollContentPresenter;
        public static int _idCount = 400;
        public static int _count = 0;
        private static int _measureCount;
        private static int _overrideCount;
        public ItemVirtualizerLogical(ItemsPresenter owner)
            : base(owner)
        {
            Id = _idCount++;
            _realizedChildren = new RealizedItems(Owner, GroupControl, Id);
            _scrollContentPresenter = Owner.FindAncestorOfType<ScrollContentPresenter>();

            //            System.Console.WriteLine($"Constructing {Id}, {Items} {++_count}");
        }

        /// <inheritdoc/>
        public override double ExtentValue
        {
            get => Items is IGroupingView gv ? gv.TotalItems + gv.TotalGroups : Items.Count();
        }

        /// <inheritdoc/>
//        public override double ViewportValue => GroupControl.LastInFullView - _scrollViewer.Offset.Y + 1;
        public override double ViewportValue => GroupControl.GetNumInView(_scrollViewer.Offset.Y);
        //{
        //    get { return GroupControl.NumInView-1; }
        //}

        /// <inheritdoc/>
        public override double ScrollValue => 1;

        /// <inheritdoc/>
        public override Size MeasureOverride(Size availableSize)
        {
            System.Console.WriteLine($"Measure Realized {_realizedChildren}  {availableSize}  {++_measureCount}");
            UpdateControls();
            if (!Owner.Bounds.Size.IsDefault)
                _realizedChildren.RemoveChildren();
            //           System.Console.WriteLine($"Measured Realized {_realizedChildren} Info {_currentState} {_estimatedSize}  {_measureCount}");
            if (VirtualizingPanel.ScrollDirection == Layout.Orientation.Vertical)
                return _estimatedSize;
            return _estimatedSize;
        }

        public override Size ArrangeOverride(Size finalSize)
        {
            if (Owner.Bounds.Size.IsDefault)
                return finalSize;
            GroupControl.RemoveGroup(Items);
            System.Console.WriteLine($"Arrange Realized {_realizedChildren}  {finalSize}  {++_overrideCount}");

           foreach (var container in _realizedChildren)
            {
                var control = container.ContainerControl;
                var startOffset = VirtualizingAverages.GetOffsetForIndex(GroupControl.TemplatedParent, container.Index, Items, Vertical);
                if (Vertical)
                    control.Arrange(new Rect(new Point(0, startOffset), new Size(finalSize.Width, control.DesiredSize.Height)));
                else
                    control.Arrange(new Rect(new Point(startOffset, 0), new Size(control.DesiredSize.Width, finalSize.Height)));
            }
            Owner.Panel.Arrange(new Rect(finalSize));
            GroupControl.AddGroup(_realizedChildren._info,Items, Owner.ItemContainerGenerator);
            if (Items is GroupingView)
            {
                if ((GroupControl.GetItemByIndex((int)_scrollViewer.Offset.Y, out var firstContainer)) && (!firstContainer.ContainerControl.Bounds.IsEmpty))
                {
                    var rel = firstContainer.ContainerControl.TranslatePoint(new Point(0, 0), _scrollViewer).Value;
                    VirtualizingPanel.AdjustPosition(rel);
                }
            }
            if (GroupControl.Changed())
                _scrollContentPresenter.InvalidateMeasure();
            //            var n=GroupControl.GetNumInView(OffsetValue);
            return finalSize;
        }


        /// <inheritdoc/>
        public override void UpdateControls()
        {
            CreateAndRemoveContainers();
        }

        /// <inheritdoc/>
        public override void ItemsChanged(IEnumerable items, NotifyCollectionChangedEventArgs e)
        {
            System.Console.WriteLine($"ItemsChanged {Id} Current Items {Items}   New Items {items} ");
            base.ItemsChanged(items, e);
            if (e.Action != NotifyCollectionChangedAction.Add)
                ItemContainerSync.ItemsChanged(Owner, null, e);
            else
                Owner.InvalidateMeasure();
        }

        /// <summary>
        /// Scrolls the specified item into view.
        /// </summary>
        /// <param name="index">The index of the item.</param>
        public override void ScrollIntoView(int index)
        {
            if (index != -1)
            {
                var container = Owner.ItemContainerGenerator.ContainerFromIndex(index);
                container?.BringIntoView();
            }
        }

        private void CreateAndRemoveContainers()
        {
            var generator = Owner.ItemContainerGenerator;
            if (Owner.Bounds.Size.IsDefault)
            {
                if ((Items.Count() > 0) && !_estimated)
                {
                    var materialized = generator.Materialize(0, Items.ElementAt(0));
                    VirtualizingPanel.Children.Insert(0, materialized.ContainerControl);
                    materialized.ContainerControl.Measure(Size.Infinity);
                    VirtualizingAverages.AddContainerSize(GroupControl.TemplatedParent, Items.ElementAt(0), materialized.ContainerControl);
                    //VirtualizingPanel.Children.RemoveAt(0);
                    //generator.Dematerialize(0, 1);
                    //ItemsPresenter.InvalidateMeasure();
                    _estimated = true;
                    System.Console.WriteLine($"Estimate {Id} Items:{Items} ");
                }
            }
            else if (Items != null && VirtualizingPanel.IsAttachedToVisualTree)
            {
                _realizedChildren.AddChildren();
                //if (_viewport != _realizedChildren.Info.NumInFullView)
                //{
                //    System.Console.WriteLine($"Viewport Changed {Id} Items:{Items} {_viewport} to {_realizedChildren.Info.NumInFullView}");
                //    _viewport = _realizedChildren.Info.NumInFullView;
                //    InvalidateScroll();
                //}
            }
            _estimatedSize = VirtualizingAverages.GetEstimatedExtent(GroupControl.TemplatedParent, Items, Vertical);
        }

        public override string ToString()
        {
            return $"{Id}";
        }

        ~ItemVirtualizerLogical()
        {
            System.Console.WriteLine($"Destructing {Id}, {Items} {--_count}");
        }
    }


}

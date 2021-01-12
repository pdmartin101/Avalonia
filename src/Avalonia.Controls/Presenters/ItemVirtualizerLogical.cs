using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Utils;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Utilities;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Presenters
{
    /// <summary>
    /// Handles virtualization in an <see cref="ItemsPresenter"/> for
    /// <see cref="ItemVirtualizationMode.Simple"/>.
    /// </summary>
    internal class ItemVirtualizerLogical : ItemVirtualizer
    {
        private VirtualizedRealizedItems _realizedChildren;
        private RealizedChildrenInfo _currentState=new RealizedChildrenInfo();
        private IControl _itemsPresenter;
        private ScrollViewer _scrollViewer;
        private int _id;
        public static int _count = 100;
        public ItemVirtualizerLogical(ItemsPresenter owner)
            : base(owner)
        {
            _scrollViewer = VirtualizingPanel.Parent.Parent as ScrollViewer;
            _scrollViewer.ScrollChanged += Scroll_ScrollChanged;
            _itemsPresenter = VirtualizingPanel.Parent;
            _id = _count++;
            _realizedChildren = new VirtualizedRealizedItems(VirtualizingPanel, _scrollViewer, Items, Owner.ItemContainerGenerator,_id);
        }

        /// <inheritdoc/>
        public override bool IsLogicalScrollEnabled => false;

        /// <inheritdoc/>
        public override double ExtentValue => 23;// VirtualizingAverages.GetEstimatedExtent(VirtualizingPanel.TemplatedParent, Items, Vertical)+8;

        /// <inheritdoc/>
        public override double OffsetValue
        {
            get
            {
                return 56;// _currentState.FirstInView * VirtualizingAverages.GetEstimatedAverage(VirtualizingPanel.TemplatedParent,Items,Vertical) + _currentState.Margin;
            }

            set
            {
//                _currentState.SetFirst(value);
            }

        }

        /// <inheritdoc/>
        public override double ViewportValue
        {
            get
            {
                return 56;// _currentState.NumInFullView * VirtualizingAverages.GetEstimatedAverage(VirtualizingPanel.TemplatedParent, Items, Vertical);
            }
        }

        /// <inheritdoc/>
        public override Size MeasureOverride(Size availableSize)
        {
            UpdateControls();
            var s = Owner.Panel.DesiredSize;
            var estimatedSize = VirtualizingAverages.GetEstimatedExtent(VirtualizingPanel.TemplatedParent, Items, Vertical);
            _realizedChildren.RemoveChildren(Vertical);
            if (VirtualizingPanel.ScrollDirection == Layout.Orientation.Vertical)
                return estimatedSize;
            return estimatedSize;
        }

        public override Size ArrangeOverride(Size finalSize)
        {
            foreach (var container in _realizedChildren)
            {
                var startOffset = VirtualizingAverages.GetOffsetForIndex(VirtualizingPanel.TemplatedParent, container.Index, Items, Vertical);
                if (Vertical)
                    container.ContainerControl.Arrange(new Rect(new Point(0, startOffset), new Size(finalSize.Width, container.ContainerControl.DesiredSize.Height)));
                else
                    container.ContainerControl.Arrange(new Rect(new Point(startOffset, 0), new Size(container.ContainerControl.DesiredSize.Width, finalSize.Height)));
            }
            Owner.Panel.Arrange(new Rect(finalSize));
            return finalSize;
        }

        /// <inheritdoc/>
        public override void UpdateControls()
        {
            CreateAndRemoveContainers();
            InvalidateScroll();
        }

        /// <inheritdoc/>
        public override void ItemsChanged(IEnumerable items, NotifyCollectionChangedEventArgs e)
        {
            base.ItemsChanged(items, e);
            ItemContainerSync.ItemsChanged(Owner, items, e);
            _itemsPresenter.InvalidateMeasure();
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
            if (_itemsPresenter.Bounds.Size.IsDefault)
            {
                var materialized = generator.Materialize(0, Items.ElementAt(0));
                VirtualizingPanel.Children.Insert(0, materialized.ContainerControl);
                materialized.ContainerControl.Measure(Size.Infinity);
                VirtualizingAverages.AddContainerSize(VirtualizingPanel.TemplatedParent, Items.ElementAt(0), materialized.ContainerControl.DesiredSize);
                VirtualizingPanel.Children.RemoveAt(0);
                generator.Dematerialize(0, 1);
            }
            else if (Items != null && VirtualizingPanel.IsAttachedToVisualTree)
            {
                _currentState= _realizedChildren.AddChildren(Vertical);
                if (_currentState.RequiresReMeasure)
                    _itemsPresenter.InvalidateMeasure();
            }
        }

        private void Scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            _itemsPresenter.InvalidateMeasure();
        }

    }
}

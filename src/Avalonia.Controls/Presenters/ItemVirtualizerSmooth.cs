using System;
using System.Collections;
using System.Collections.Specialized;
using Avalonia.Collections;
using Avalonia.Controls.Utils;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Presenters
{
    /// <summary>
    /// Represents an item virtualizer for an <see cref="ItemsPresenter"/> that doesn't actually
    /// virtualize items - it just creates a container for every item.
    /// </summary>
    internal class ItemVirtualizerSmooth : ItemVirtualizer
    {
        private VirtualizedRealizedItems _realizedChildren;
        private RealizedChildrenInfo _currentState = new RealizedChildrenInfo();
        private IControl _itemsPresenter;
//        private ScrollViewer _scrollViewer;
        private int _id;
        public static int _count=300;
        public ItemVirtualizerSmooth(ItemsPresenter owner)
            : base(owner)
        {
//            _scrollViewer = VirtualizingPanel.Parent.Parent as ScrollViewer;
            var scrollViewer = VirtualizingPanel.FindAncestorOfType<ScrollViewer>();
//            _scrollViewer.ScrollChanged += Scroll_ScrollChanged;
            _itemsPresenter = VirtualizingPanel.Parent;
            _id = _count++;
            _realizedChildren = new VirtualizedRealizedItems(VirtualizingPanel,scrollViewer,Items,Owner.ItemContainerGenerator,_id);
        }

        protected override IEnumerable GetItems()
        {
            if ((Owner.Items is GroupViewList gvl) && (gvl.IsGrouping))
                return gvl.Groups;
            return base.GetItems();
        }

        /// <inheritdoc/>
        public override bool IsLogicalScrollEnabled => false;

        /// <summary>
        /// This property should never be accessed because <see cref="IsLogicalScrollEnabled"/> is
        /// false.
        /// </summary>
        public override double ExtentValue
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// This property should never be accessed because <see cref="IsLogicalScrollEnabled"/> is
        /// false.
        /// </summary>
        public override double OffsetValue
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// This property should never be accessed because <see cref="IsLogicalScrollEnabled"/> is
        /// false.
        /// </summary>
        public override double ViewportValue
        {
            get { return 999; }
        }

        /// <inheritdoc/>
        public override Size MeasureOverride(Size availableSize)
        {
            //            Owner.Panel.Measure(availableSize);
//            System.Console.WriteLine($"SmoothMeasure {_id} {availableSize}");
            UpdateControls();
            var s = Owner.Panel.DesiredSize;
            var estimatedSize = VirtualizingAverages.GetEstimatedExtent(VirtualizingPanel.TemplatedParent,Items,Vertical);
            _realizedChildren.RemoveChildren(Vertical);
            if (VirtualizingPanel.ScrollDirection== Layout.Orientation.Vertical)
                return new Size(s.Width, estimatedSize);
            return new Size(estimatedSize, s.Height);
        }

        public override Size ArrangeOverride(Size finalSize)
        {
            foreach (var container in _realizedChildren)
            {
                var startOffset = VirtualizingAverages.GetOffsetForIndex(VirtualizingPanel.TemplatedParent, container.Index, Items,Vertical);
                if (Vertical)
                    container.ContainerControl.Arrange(new Rect(new Point(0,startOffset),new Size(finalSize.Width, container.ContainerControl.DesiredSize.Height)));
                else
                    container.ContainerControl.Arrange(new Rect(new Point(startOffset,0), new Size(container.ContainerControl.DesiredSize.Width, finalSize.Height )));
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
//                System.Console.WriteLine($"SmoothDummy {_id} {materialized.ContainerControl.DesiredSize} {_scrollViewer.Bounds.Height}");
                VirtualizingAverages.AddContainerSize(VirtualizingPanel.TemplatedParent, Items.ElementAt(0), materialized.ContainerControl.DesiredSize);
                VirtualizingPanel.Children.RemoveAt(0);
                generator.Dematerialize(0, 1);
                _itemsPresenter.InvalidateMeasure();
            }
            else if (Items != null && VirtualizingPanel.IsAttachedToVisualTree)
            {
                _currentState=_realizedChildren.AddChildren(Vertical);
                if (_currentState.RequiresReMeasure)
                    _itemsPresenter.InvalidateMeasure();
            }
        }

//        private void Scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
//        {
////            System.Console.WriteLine($"Smooth Scroll {_id} {e.ExtentDelta} {e.OffsetDelta} {e.ViewportDelta}");
////            _itemsPresenter.InvalidateMeasure();
//        }

    }

 
}

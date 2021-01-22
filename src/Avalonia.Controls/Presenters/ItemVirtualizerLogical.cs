using System.Collections;
using System.Collections.Specialized;
using System.Reactive.Linq;
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
        private VirtualizedRealizedItems _realizedChildren;
        private RealizedChildrenInfo _currentState;
        public int Id;
        private bool _estimated;
        private Size _estimatedSize;
        private double _viewport;
        public static int _idCount = 400;
        public static int _count = 0;
        private static int _measureCount;
        private static int _overrideCount;
        public ItemVirtualizerLogical(ItemsPresenter owner)
            : base(owner)
        {
            _currentState = new RealizedChildrenInfo(Vertical);
            var scrollViewer = VirtualizingPanel.FindAncestorOfType<ScrollViewer>();
            Id = _idCount++;
            _realizedChildren = new VirtualizedRealizedItems(Owner, Id);
            System.Console.WriteLine($"Constructing {Id}, {Items} {++_count}");
        }

        /// <inheritdoc/>
        public override double ExtentValue
        {
            get=>ItemCount;
        }

        /// <inheritdoc/>
        public override double ViewportValue
        {
            get { return _viewport; }
        }

        /// <inheritdoc/>
        public override double ScrollValue => 1;

        /// <inheritdoc/>
        public override Size MeasureOverride(Size availableSize)
        {
            System.Console.WriteLine($"Measure Realized {_realizedChildren} Info {_currentState} {availableSize}  {++_measureCount}");
            UpdateControls();
            if (!Owner.Bounds.Size.IsDefault)
                _realizedChildren.RemoveChildren(_currentState);
            System.Console.WriteLine($"Measured Realized {_realizedChildren} Info {_currentState} {_estimatedSize}  {_measureCount}");
            if (VirtualizingPanel.ScrollDirection == Layout.Orientation.Vertical)
                return _estimatedSize;
            return _estimatedSize;
        }

        public override Size ArrangeOverride(Size finalSize)
        {
//            System.Console.WriteLine($"Override {Id}  {finalSize}  {_realizedChildren.Count}  {++_overrideCount}");
            var startOffset = _currentState.PanelOffset < 0?0: _currentState.PanelOffset;
            for (int i = _currentState.FirstInView; i < _currentState.LastInView+1; i++)
            {
                var control=_realizedChildren.ContainerFromIndex(i);
                if (Vertical)
                {
                    control.Arrange(new Rect(new Point(0, startOffset), new Size(finalSize.Width, control.DesiredSize.Height)));
                    startOffset += control.DesiredSize.Height;
                }
                else
                {
                    control.Arrange(new Rect(new Point(startOffset,0), new Size(control.DesiredSize.Width, finalSize.Height)));
                    startOffset += control.DesiredSize.Width;
                }
            }
            Owner.Panel.Arrange(new Rect(finalSize));
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
                    var desiredItemSize = materialized.ContainerControl.DesiredSize;
                    VirtualizingAverages.AddContainerSize(VirtualizingPanel.TemplatedParent, Items.ElementAt(0), desiredItemSize);
                    //VirtualizingPanel.Children.RemoveAt(0);
                    //generator.Dematerialize(0, 1);
                    //ItemsPresenter.InvalidateMeasure();
                    _estimated = true;
                    System.Console.WriteLine($"Estimate {Id} Items:{Items} ");
                }
            }
            else if (Items != null && VirtualizingPanel.IsAttachedToVisualTree)
            {
                _realizedChildren.AddChildren(_currentState);
                if (_viewport != _currentState.NumInFullView)
                {
                    _viewport = _currentState.NumInFullView;
                    InvalidateScroll();
                }
            }
            _estimatedSize = VirtualizingAverages.GetEstimatedExtent(VirtualizingPanel.TemplatedParent, Items, Vertical);
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

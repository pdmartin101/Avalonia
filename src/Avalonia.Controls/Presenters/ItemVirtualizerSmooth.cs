using System.Collections;
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
    internal class ItemVirtualizerSmooth : ItemVirtualizer
    {
        private RealizedItems _realizedChildren;
        public int Id;
        private bool _estimated;
        private Size _estimatedSize;
        public static int _idCount = 300;
        public static int _gcount = 0;
        private static int _measureCount;
        private static int _overrideCount;
        public ItemVirtualizerSmooth(ItemsPresenter owner)
            : base(owner)
        {
            Id = _idCount++;
            _realizedChildren = new RealizedItems(Owner,GroupControl, Id);
            PdmLogger.Log(30, PdmLogger.IndentEnum.Nothing, $"Constructing {Id}, {Items} {++_gcount}");
        }

        /// <inheritdoc/>
        public override Size MeasureOverride(Size availableSize)
        {
            PdmLogger.Log(0, PdmLogger.IndentEnum.In, $"Measure Realized {_realizedChildren}  {availableSize}  {++_measureCount}  {!Owner.Bounds.Size.IsDefault}");
            UpdateControls();
            if (!_scrollViewer.Bounds.Size.IsDefault)
                _realizedChildren.RemoveChildren();
            PdmLogger.Log(1, PdmLogger.IndentEnum.Out, $"Measured Realized {_realizedChildren}  {_estimatedSize}  {_measureCount}  {!Owner.Bounds.Size.IsDefault}");
            if (VirtualizingPanel.ScrollDirection == Layout.Orientation.Vertical)
                return _estimatedSize;
            return _estimatedSize;
        }

        public override Size ArrangeOverride(Size finalSize)
        {
            PdmLogger.Log(0, PdmLogger.IndentEnum.In, $"Arrange Realized {_realizedChildren}  {finalSize}  {++_overrideCount}  {Owner.Bounds}");
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
            PdmLogger.Log(1, PdmLogger.IndentEnum.Out, $"Arranged Realized {_realizedChildren}  {finalSize}  {_overrideCount}  {Owner.Bounds}");
            return finalSize;
        }

        /// <inheritdoc/>
        public override void UpdateControls()
        {
            CreateContainers();
        }

        /// <inheritdoc/>
        public override void ItemsChanged(IEnumerable items, NotifyCollectionChangedEventArgs e)
        {
            PdmLogger.Log(2, PdmLogger.IndentEnum.Nothing, $"ItemsChanged {Id} Current Items {Items}   New Items {items} ");
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

        private void CreateContainers()
        {
            var generator = Owner.ItemContainerGenerator;
            if (Owner.Bounds.Size.IsDefault)
            {
                if ((Items.Count() > 0) && !_estimated)
                {
                    PdmLogger.Log(0, PdmLogger.IndentEnum.In, $"Estimate {Id} Items:{Items} ");
                    var materialized = generator.Materialize(0, Items.ElementAt(0));
                    VirtualizingPanel.Children.Insert(0, materialized.ContainerControl);
                    materialized.ContainerControl.Measure(Size.Infinity);
                    VirtualizingAverages.AddContainerSize(GroupControl.TemplatedParent, Items.ElementAt(0), materialized.ContainerControl);
                    //VirtualizingPanel.Children.RemoveAt(0);
                    //generator.Dematerialize(0, 1);
                    //ItemsPresenter.InvalidateMeasure();
                    _estimated = true;
                    PdmLogger.Log(1, PdmLogger.IndentEnum.Out, $"Estimated {Id} Items:{Items} ");
                }
            }
            else if (Items != null && VirtualizingPanel.IsAttachedToVisualTree)
            {
                _realizedChildren.AddChildren();
            }
            _estimatedSize = VirtualizingAverages.GetEstimatedExtent(GroupControl.TemplatedParent, Items, Vertical);
        }

        public override string ToString()
        {
            return $"{Id}";
        }

        ~ItemVirtualizerSmooth()
        {
            PdmLogger.Log(31, PdmLogger.IndentEnum.Nothing, $"Destructing Smooth {Id}, {Items} {--_gcount}");
        }
    }


}

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Utils;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Presenters
{
    /// <summary>
    /// Base class for classes which handle virtualization for an <see cref="ItemsPresenter"/>.
    /// </summary>
    internal abstract class ItemVirtualizer : IVirtualizingController, IDisposable
    {
        private double _crossAxisOffset;
        private IDisposable _subscriptions;
        private IDisposable _extentSub;
        private IDisposable _viewportSub;
        protected ScrollViewer _scrollViewer;
        private Rect _lastExtent;
        private Rect _lastViewport;
        private Rect _lastVirt;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemVirtualizer"/> class.
        /// </summary>
        /// <param name="owner"></param>
        public ItemVirtualizer(ItemsPresenter owner)
        {
            Owner = owner;
            Items = GetItems();
            ItemCount = Items.Count();
            _scrollViewer = VirtualizingPanel.FindAncestorOfType<ScrollViewer>();

            var panel = VirtualizingPanel;

            if (panel != null)
            {
                //_subscriptions = panel.GetObservable(Panel.BoundsProperty)
                //    .Skip(1)
                //    .Subscribe(o => VirtChanged(o));
                _extentSub = owner.GetObservable(Panel.BoundsProperty)
                    .Skip(1)
                    .Subscribe(o => ExtentChanged(o));
                _viewportSub = _scrollViewer.GetObservable(ScrollViewer.BoundsProperty)
                    .Skip(1)
                    .Subscribe(o => ViewportChanged(o));
            }
        }

        protected virtual IEnumerable GetItems()
        {
            return Owner.Items;
        }
        /// <summary>
        /// Gets the <see cref="ItemsPresenter"/> which owns the virtualizer.
        /// </summary>
        public ItemsPresenter Owner { get; }

        /// <summary>
        /// Gets the <see cref="IVirtualizingPanel"/> which will host the items.
        /// </summary>
        public IVirtualizingPanel VirtualizingPanel => Owner.Panel as IVirtualizingPanel;

        /// <summary>
        /// Gets the items to display.
        /// </summary>
        public IEnumerable Items { get; private set; }

        /// <summary>
        /// Gets the number of items in <see cref="Items"/>.
        /// </summary>
        public int ItemCount { get; private set; }

        /// <summary>
        /// Gets or sets the index of the first item displayed in the panel.
        /// </summary>
        public int FirstIndex { get; protected set; }

        /// <summary>
        /// Gets or sets the index of the first item beyond those displayed in the panel.
        /// </summary>
        public int NextIndex { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the items should be scroll horizontally or vertically.
        /// </summary>
        public bool Vertical => VirtualizingPanel?.ScrollDirection == Orientation.Vertical;

        /// <summary>
        /// Gets a value indicating whether logical scrolling is enabled.
        /// </summary>
        public virtual bool IsLogicalScrollEnabled => false;

        /// <summary>
        /// Gets the value of the scroll extent.
        /// </summary>
        public virtual double ExtentValue => Vertical?Owner.DesiredSize.Height: Owner.DesiredSize.Width;  // PDMPDM maybe should be VirtPanel

        /// <summary>
        /// This property should never be accessed because <see cref="IsLogicalScrollEnabled"/> is
        /// false.
        /// </summary>
        public virtual double OffsetValue { get; set; }

        /// <summary>
        /// Gets the value of the scrollable viewport.
        /// </summary>
        public virtual double ViewportValue => GetViewPort();

        private double GetViewPort()
        {
            if (_scrollViewer != null)
                return Vertical ?_scrollViewer.Viewport.Height:_scrollViewer.Viewport.Width;
            return Vertical ? Owner.Bounds.Height:Owner.Bounds.Width;
        }

        /// <summary>
        /// Gets the value of the small scroll step.
        /// </summary>
        public virtual double ScrollValue => ScrollViewer.DefaultSmallChange;

        /// <summary>
        /// Gets the <see cref="ExtentValue"/> as a <see cref="Size"/>.
        /// </summary>
        public Size Extent=> Vertical ?
                        new Size(Owner.DesiredSize.Width, ExtentValue) :
                        new Size(ExtentValue, Owner.DesiredSize.Height);

        /// <summary>
        /// Gets the <see cref="ViewportValue"/> as a <see cref="Size"/>.
        /// </summary>
        public Size Viewport => Vertical ?
                        Owner.Bounds.Size.Inflate(Owner.Margin).WithHeight(ViewportValue) :
                        new Size(ViewportValue, Owner.Bounds.Height + Owner.Margin.Top + Owner.Margin.Bottom);
 
        /// <summary>
        /// Gets or sets the <see cref="OffsetValue"/> as a <see cref="Vector"/>.
        /// </summary>
        public Vector Offset
        {
            get
            {
                if (IsLogicalScrollEnabled)
                {
                    return Vertical ? new Vector(_crossAxisOffset, OffsetValue) : new Vector(OffsetValue, _crossAxisOffset);
                }
                return Vertical ? new Vector(0, OffsetValue) : new Vector(OffsetValue, 0);
            }

            set
            {
                if (!IsLogicalScrollEnabled)
                {
                    OffsetValue=Vertical?value.Y:value.X;
                }

                var oldCrossAxisOffset = _crossAxisOffset;

                if (Vertical)
                {
                    OffsetValue = value.Y;
                    _crossAxisOffset = value.X;
                }
                else
                {
                    OffsetValue = value.X;
                    _crossAxisOffset = value.Y;
                }

                if (_crossAxisOffset != oldCrossAxisOffset)
                {
                    Owner.InvalidateArrange();
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="ScrollSize"/> as a <see cref="Size"/>.
        /// </summary>
        public Size ScrollSize => Vertical ?
                        new Size(ScrollViewer.DefaultSmallChange, ScrollValue) :
                        new Size(ScrollValue, 0);


        /// <summary>
        /// Creates an <see cref="ItemVirtualizer"/> based on an item presenter's 
        /// <see cref="ItemVirtualizationMode"/>.
        /// </summary>
        /// <param name="owner">The items presenter.</param>
        /// <returns>An <see cref="ItemVirtualizer"/>.</returns>
        public static ItemVirtualizer Create(ItemsPresenter owner)
        {
            if (owner.Panel == null)
            {
                return null;
            }

            var virtualizingPanel = owner.Panel as IVirtualizingPanel;
            var scrollContentPresenter = owner.Parent as IScrollable;
            ItemVirtualizer result = null;

//            if (virtualizingPanel != null && scrollContentPresenter is object)
              if (virtualizingPanel != null)
                {
                    switch (owner.VirtualizationMode)
                {
                    case ItemVirtualizationMode.Simple:
                        result = new ItemVirtualizerSimple(owner);
                        break;
                    case ItemVirtualizationMode.Logical:
                        result = new ItemVirtualizerLogical(owner);
                        break;
                    case ItemVirtualizationMode.Smooth:
                        result = new ItemVirtualizerSmooth(owner);
                        break;
                }
            }

            if (result == null)
            {
                result = new ItemVirtualizerNone(owner);
            }

            if (virtualizingPanel != null)
            {
                virtualizingPanel.Controller = result;
            }

            return result;
        }

        /// <summary>
        /// Carries out a measure for the related <see cref="ItemsPresenter"/>.
        /// </summary>
        /// <param name="availableSize">The size available to the control.</param>
        /// <returns>The desired size for the control.</returns>
        public virtual Size MeasureOverride(Size availableSize)
        {
            Owner.Panel.Measure(availableSize);
            return Owner.Panel.DesiredSize;
        }

        /// <summary>
        /// Carries out an arrange for the related <see cref="ItemsPresenter"/>.
        /// </summary>
        /// <param name="finalSize">The size available to the control.</param>
        /// <returns>The actual size used.</returns>
        public virtual Size ArrangeOverride(Size finalSize)
        {
            if (VirtualizingPanel != null)
            {
                VirtualizingPanel.CrossAxisOffset = _crossAxisOffset;
                Owner.Panel.Arrange(new Rect(finalSize));
            }
            else
            {
                var origin = Vertical ? new Point(-_crossAxisOffset, 0) : new Point(0, _crossAxisOffset);
                Owner.Panel.Arrange(new Rect(origin, finalSize));
            }

            return finalSize;
        }

        /// <inheritdoc/>
        public virtual void UpdateControls()
        {
        }

        /// <summary>
        /// Gets the next control in the specified direction.
        /// </summary>
        /// <param name="direction">The movement direction.</param>
        /// <param name="from">The control from which movement begins.</param>
        /// <returns>The control.</returns>
        public virtual IControl GetControlInDirection(NavigationDirection direction, IControl from)
        {
            return null;
        }

        /// <summary>
        /// Called when the items for the presenter change, either because 
        /// <see cref="ItemsPresenterBase.Items"/> has been set, the items collection has been
        /// modified, or the panel has been created.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="e">A description of the change.</param>
        public virtual void ItemsChanged(IEnumerable items, NotifyCollectionChangedEventArgs e)
        {
            Items = items;
            ItemCount = items.Count();
        }

        /// <summary>
        /// Scrolls the specified item into view.
        /// </summary>
        /// <param name="index">The index of the item.</param>
        public virtual void ScrollIntoView(int index)
        {
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            _scrollViewer = null;
            System.Console.WriteLine($"Dispose {Items}");
            _subscriptions?.Dispose();
            _subscriptions = null;
            _extentSub?.Dispose();
            _extentSub = null;
            _viewportSub?.Dispose();
            _viewportSub = null;

            if (VirtualizingPanel != null)
            {
                VirtualizingPanel.Controller = null;
                VirtualizingPanel.Children.Clear();
            }

            Owner.ItemContainerGenerator.Clear();
        }

        /// <summary>
        /// Invalidates the current scroll.
        /// </summary>
        protected void InvalidateScroll() => ((ILogicalScrollable)Owner).RaiseScrollInvalidated(EventArgs.Empty);

        private void ExtentChanged(Rect bounds)
        {
//            System.Console.WriteLine($"Extent from  {_lastExtent}  to {bounds}");
            if (_lastExtent != bounds)
                Owner.InvalidateMeasure();
            _lastExtent = bounds;
        }
        private void ViewportChanged(Rect bounds)
        {
//            System.Console.WriteLine($"Viewport from  {_lastViewport}  to {bounds}");
            if (_lastViewport != bounds)
                Owner.InvalidateMeasure();
            _lastViewport = bounds;
        }
        private void VirtChanged(Rect bounds)
        {
            System.Console.WriteLine($"Virt from  {_lastVirt}  to {bounds}");
            if (_lastVirt != bounds)
                Owner.InvalidateMeasure();
            _lastVirt = bounds;
        }

    }
}

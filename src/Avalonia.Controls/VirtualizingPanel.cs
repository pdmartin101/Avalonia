using System;
using Avalonia.Input;
using Avalonia.Layout;

namespace Avalonia.Controls
{
    public class VirtualizingPanel : Panel, IVirtualizingPanel
    {
        /// <summary>
        /// Defines the <see cref="Spacing"/> property.
        /// </summary>
        public static readonly StyledProperty<double> SpacingProperty =
            StackLayout.SpacingProperty.AddOwner<StackPanel>();

        /// <summary>
        /// Defines the <see cref="Orientation"/> property.
        /// </summary>
        public static readonly StyledProperty<Orientation> OrientationProperty =
            StackLayout.OrientationProperty.AddOwner<StackPanel>();

        /// <summary>
        /// Initializes static members of the <see cref="StackPanel"/> class.
        /// </summary>
        static VirtualizingPanel()
        {
            AffectsMeasure<StackPanel>(SpacingProperty);
            AffectsMeasure<StackPanel>(OrientationProperty);
        }

        /// <summary>
        /// Gets or sets the size of the spacing to place between child controls.
        /// </summary>
        public double Spacing
        {
            get { return GetValue(SpacingProperty); }
            set { SetValue(SpacingProperty, value); }
        }

        /// <summary>
        /// Gets or sets the orientation in which child controls will be layed out.
        /// </summary>
        public Orientation Orientation
        {
            get { return GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

 
        /// <summary>
        /// Content arrangement.
        /// </summary>
        /// <param name="finalSize">Arrange size</param>
        protected override Size ArrangeOverride(Size finalSize)
        {
            return finalSize;
        }

        IVirtualizingController IVirtualizingPanel.Controller { get; set; }

        bool IVirtualizingPanel.IsFull => throw new NotImplementedException();

        int IVirtualizingPanel.OverflowCount => throw new NotImplementedException();

        Orientation IVirtualizingPanel.ScrollDirection => Orientation;

        double IVirtualizingPanel.AverageItemSize => throw new NotImplementedException();

        double IVirtualizingPanel.PixelOverflow => throw new NotImplementedException();

        double IVirtualizingPanel.PixelOffset { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        double IVirtualizingPanel.CrossAxisOffset { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        void IVirtualizingPanel.ForceInvalidateMeasure()
        {
            throw new NotImplementedException();
        }

    }
}

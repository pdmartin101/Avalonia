using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.Controls
{
    class GroupStackPanelOuter : VirtualizingStackPanel
    {
        public GroupStackPanelOuter()
        {
            
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var avail = base.MeasureOverride(availableSize);
            return avail;
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            return base.ArrangeOverride(finalSize);
        }
    }
}

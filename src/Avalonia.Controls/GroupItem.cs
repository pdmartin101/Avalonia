using System;
using Avalonia.Collections;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Styling;

namespace Avalonia.Controls
{
    /// <summary>
    /// A selectable item in a <see cref="GroupItem"/>.
    /// </summary>
    public class GroupItem : ItemsControl, IStyleable
    {
        public int Level { get; set; }
        public ItemVirtualizationMode VirtualizationMode { get; set; } = ItemVirtualizationMode.Smooth;
        public ItemsControl GroupParent { get; set; }
        public IPanel VirtualizingPanel => Presenter?.Panel;
        public static int _count = 0;


        public GroupItem(ItemsControl itemsControl)
        {
            if (itemsControl is GroupItem gi)
                Level = gi.Level + 1;
            Name = $"Level{Level,2:00}";
            _count++;
        }

        protected override IItemContainerGenerator CreateItemContainerGenerator()
        {
            if (Presenter is ItemsPresenter ip)
            {
                ip.VirtualizationMode = VirtualizationMode;
                ip.SetValue(ItemsPresenter.ItemsPanelProperty, ItemsPanel);   // this can be done with a template binding in style, but this simplifies xaml
            }
            if ((Items is IGroupingView igv) && (igv.IsGrouping))
                return new GroupContainerGenerator(this, GroupParent);
            var container = GroupParent.CreateLeafItemContainerGenerator();
            return container;
        }

        //protected override Size MeasureOverride(Size availableSize)
        //{
        //    //System.Console.WriteLine($"PresenterInvalidate");
        //    //Presenter.InvalidateMeasure();
        //    return base.MeasureOverride(availableSize);
        //}
        Type IStyleable.StyleKey => typeof(GroupItem);

        //static GroupItem()
        //{
        //    PressedMixin.Attach<GroupItem>();
        //    FocusableProperty.OverrideDefaultValue<GroupItem>(true);
        //}
        ~GroupItem()
        {
            System.Console.WriteLine($"Destructing GroupItem, {Items}  {--_count}");
        }

    }

}

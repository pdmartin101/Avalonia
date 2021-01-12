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
    public abstract class GroupItem : ItemsControl
    {
        public int Level { get; set; }

        public GroupItem(ItemsControl itemsControl)
        {
            if (itemsControl is GroupItem gi)
                Level = gi.Level + 1;
            Name = $"Level{Level,2:00}";
        }

        protected override IItemContainerGenerator CreateTopItemContainerGenerator()
        {
            return new ItemContainerGenerator<ListBoxItem>(
                this,
                ListBoxItem.ContentProperty,
                ListBoxItem.ContentTemplateProperty);
        }

        /// <summary>
        /// Initializes static members of the <see cref="GroupItem"/> class.
        /// </summary>
        /// 

        static GroupItem()
        {
            PressedMixin.Attach<GroupItem>();
            FocusableProperty.OverrideDefaultValue<GroupItem>(true);
        }

    }

    public class GroupItem<T> : GroupItem, IStyleable where T : class, IControl, new()
    {
        public GroupItem(ItemsControl itemsControl) : base(itemsControl)
        {
        }

        public ItemVirtualizationMode VirtualizationMode { get; set; } = ItemVirtualizationMode.Smooth;
        public ItemsControl GroupParent { get; set; }

        protected override IItemContainerGenerator CreateTopItemContainerGenerator()
        {
            if (Presenter is ItemsPresenter ip)
            {
                ip.VirtualizationMode = VirtualizationMode;
                ip.SetValue(ItemsPresenter.ItemsPanelProperty, ItemsPanel);   // this can be done with a template binding in style, but this simplifies xaml
            }
            if ((Items is GroupingViewInternal gvl) && (gvl.IsGrouping))
                return new GroupContainerGenerator<T>(this, GroupParent);
            var container = new ItemContainerGenerator<T>(
                this,
                ListBoxItem.ContentProperty,
                ListBoxItem.ContentTemplateProperty);
 //           GroupParent.SetItemContainerGenerator(container);
            return container;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            //System.Console.WriteLine($"PresenterInvalidate");
            //Presenter.InvalidateMeasure();
            return base.MeasureOverride(availableSize);
        }
        Type IStyleable.StyleKey => typeof(GroupItem);

        /// <summary>
        /// Initializes static members of the <see cref="GroupItem"/> class.
        /// </summary>

        static GroupItem()
        {
        }

    }

    //public class GroupItem<T> : GroupItem
    //{
    //    static GroupItem()
    //    {
    //        PressedMixin.Attach<GroupItem<T>>();
    //        FocusableProperty.OverrideDefaultValue<GroupItem<T>>(true);
    //    }

    //}
}

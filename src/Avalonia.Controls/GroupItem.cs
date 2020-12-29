using System;
using Avalonia.Collections;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Presenters;
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

        /// <summary>
        /// Defines the <see cref="VirtualizationMode"/> property.
        /// </summary>
        //public static readonly StyledProperty<ItemVirtualizationMode> VirtualizationModeProperty =
        //    ItemsPresenter.VirtualizationModeProperty.AddOwner<GroupItem>();

        /// <summary>
        /// Gets or sets the virtualization mode for the items.
        /// </summary>
        //public ItemVirtualizationMode VirtualizationMode
        //{
        //    get { return GetValue(VirtualizationModeProperty); }
        //    set { SetValue(VirtualizationModeProperty, value); }
        //}
        static GroupItem()
        {
            PressedMixin.Attach<GroupItem>();
            FocusableProperty.OverrideDefaultValue<GroupItem>(true);
            //            VirtualizationModeProperty.OverrideDefaultValue<GroupItem>(ItemVirtualizationMode.Simple);
        }

    }

    public class GroupItem<T> : GroupItem, IStyleable where T : class, IControl, new()
    {
        public GroupItem(ItemsControl itemsControl) : base(itemsControl)
        {
        }
        protected override IItemContainerGenerator CreateTopItemContainerGenerator()
        {
            if (Presenter is ItemsPresenter ip)
                ip.VirtualizationMode = ItemVirtualizationMode.Smooth;
            if ((Items is GroupViewListItem gvl) && (gvl.IsGrouping))
                return new GroupContainerGenerator<T>(this);
            return new ItemContainerGenerator<T>(
                this,
                ListBoxItem.ContentProperty,
                ListBoxItem.ContentTemplateProperty);
        }


        /// <summary>
        /// Initializes static members of the <see cref="GroupListBoxItem"/> class.
        /// </summary>

        Type IStyleable.StyleKey => typeof(GroupItem);

        static GroupItem()
        {
            //            PressedMixin.Attach<GroupListBoxItem>();
            //            FocusableProperty.OverrideDefaultValue<GroupListBoxItem>(true);
            //            VirtualizationModeProperty.OverrideDefaultValue<GroupListBoxItem>(ItemVirtualizationMode.Simple);
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

using System;
using Avalonia.Collections;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Avalonia.Controls
{
    /// <summary>
    /// A selectable item in a <see cref="GroupItem"/>.
    /// </summary>
    public class GroupItem : ItemsControl, IStyleable
    {
        public int Level { get; set; }
        public ItemVirtualizationMode VirtualizationMode { get; set; } = ItemVirtualizationMode.Smooth;
        public ItemVirtualizingCache VirtualizingCache { get; set; }
//        public GroupController GroupControl { get; set; }
        public IPanel VirtualizingPanel => Presenter?.Panel;
        public static int _idCount = 0;
        public static int _gcount = 0;
        public int Id;

        public GroupItem(ItemsControl itemsControl, IGroupingView items)
        {
            if (itemsControl is GroupItem gi)
                Level = gi.Level + 1;
            Name = $"Level{Level,2:00}";
            Id = _idCount++;
            Items = items;
            PdmLogger.Log(30, PdmLogger.IndentEnum.Nothing, $"Construct GroupItem   {Id} {Items} {++_gcount}");
        }

        protected override IItemContainerGenerator CreateItemContainerGenerator()
        {
            if (Presenter is ItemsPresenter ip)
            {
                ip.VirtualizationMode = VirtualizationMode;
                ip.VirtualizingCache = VirtualizingCache;
                ip.SetValue(ItemsPresenter.ItemsPanelProperty, ItemsPanel);   // this can be done with a template binding in style, but this simplifies xaml
            }
            if ((Items is IGroupingView igv) && (igv.IsGrouping))
                return new GroupContainerGenerator(this);
            var ownerPresenter = this.FindAncestorOfType<ItemsPresenter>();
            var gc = ownerPresenter.Virtualizer.GroupControl;
            var container = gc.TemplatedParent.CreateLeafItemContainerGenerator();
            return container;
        }

        //protected override Size MeasureOverride(Size availableSize)
        //{
        //    //Presenter.InvalidateMeasure();
        //    return base.MeasureOverride(availableSize);
        //}
        Type IStyleable.StyleKey => typeof(GroupItem);

        //static GroupItem()
        //{
        //    PressedMixin.Attach<GroupItem>();
        //    FocusableProperty.OverrideDefaultValue<GroupItem>(true);
        //}

        public override string ToString()
        {
            return $"{Id} {Items}";
        }
        ~GroupItem()
        {
            PdmLogger.Log(31, PdmLogger.IndentEnum.Nothing, $"Destructing GroupItem,  {Id} {Items} {--_gcount}");
        }

    }

}

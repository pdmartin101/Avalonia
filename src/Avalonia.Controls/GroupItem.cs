using Avalonia.Controls.Generators;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Presenters;
using Avalonia.Input;

namespace Avalonia.Controls
{
    /// <summary>
    /// A selectable item in a <see cref="GroupItem"/>.
    /// </summary>
    public class GroupItem : ItemsControl
    {
        public GroupItem()
        {
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

    //public class GroupItem<T> :GroupItem
    //{
    //    static GroupItem()
    //    {
    //        PressedMixin.Attach<GroupItem<T>>();
    //        FocusableProperty.OverrideDefaultValue<GroupItem<T>>(true);
    //    }

    //}
}

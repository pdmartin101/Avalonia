using Avalonia.Collections;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Input;

namespace Avalonia.Controls
{
    /// <summary>
    /// A selectable item in a <see cref="ListBox"/>.
    /// </summary>
    [PseudoClasses(":pressed", ":selected")]
    public class ListBoxItem : ContentControl, ISelectable
    {
        /// <summary>
        /// Defines the <see cref="IsSelected"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsSelectedProperty =
            AvaloniaProperty.Register<ListBoxItem, bool>(nameof(IsSelected));
        public static int _idCount = 0;
        public static int _gcount = 0;
        public int Id;
        public string str="null";

        public ListBoxItem()
        {
            Id = _idCount++;
            PdmLogger.Log(30, PdmLogger.IndentEnum.Nothing, $"Constructing ListBoxItem, {Id}  {++_gcount}");
        }
        /// <summary>
        /// Initializes static members of the <see cref="ListBoxItem"/> class.
        /// </summary>
        static ListBoxItem()
        {
            SelectableMixin.Attach<ListBoxItem>(IsSelectedProperty);
            PressedMixin.Attach<ListBoxItem>();
            FocusableProperty.OverrideDefaultValue<ListBoxItem>(true);
        }

        /// <summary>
        /// Gets or sets the selection state of the item.
        /// </summary>
        public bool IsSelected
        {
            get { return GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            str = $"{DataContext}";
            return base.ArrangeOverride(finalSize);
        }
        public override string ToString()
        {
            return $"{Id} {str}";
        }
        ~ListBoxItem()
        {
            PdmLogger.Log(31, PdmLogger.IndentEnum.Nothing, $"Destructing ListBoxItem, {Id} {str}  {--_gcount}");
        }
    }
}

using System;
using System.ComponentModel;
using Avalonia.Collections;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Data;

namespace Avalonia.Controls.Generators
{
    /// <summary>
    /// Creates containers for items and maintains a list of created containers.
    /// </summary>
    /// <typeparam name="T">The type of the container.</typeparam>
    public class GroupContainerGenerator<T> : ItemContainerGenerator where T : class, IControl, new()
    {
        private ItemsControl _groupParent;
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemContainerGenerator{T}"/> class.
        /// </summary>
        /// <param name="owner">The owner control.</param>
        public GroupContainerGenerator(IControl owner, ItemsControl groupParent)
            : base(owner)
        {
            Contract.Requires<ArgumentNullException>(owner != null);
            _groupParent = groupParent;
        }

        /// <inheritdoc/>
        public override Type ContainerType => typeof(GroupItem);

        /// <inheritdoc/>
        protected override IControl CreateContainer(object item)
        {
            var container = item as GroupItem<T>;

            if (container != null)
                return container;
            else
            {
                var itemsControl = Owner as ItemsControl;
                var presenter = itemsControl.Presenter as ItemsPresenter;
                var result = new GroupItem<T>(itemsControl);
                result.SetValue(GroupItem.TemplatedParentProperty, Owner,BindingPriority.TemplatedParent);
                result.GroupParent = _groupParent;
                result.Items = (GroupViewListItem)item;
                result.SetValue(GroupItem.ItemsPanelProperty, itemsControl.ItemsPanel);
                result.ItemTemplate = itemsControl?.ItemTemplate;
                if (presenter !=null)
                    result.VirtualizationMode = presenter.VirtualizationMode;
                if (!(item is IControl))
                    result.DataContext = item;

                return result;
            }
        }

        /// <inheritdoc/>
        public override bool TryRecycle(int oldIndex, int newIndex, object item)
        {
            var container = ContainerFromIndex(oldIndex);

            if (container == null)
            {
                throw new IndexOutOfRangeException("Could not recycle container: not materialized.");
            }

            if (!(item is IControl))
            {
                container.DataContext = item;
            }

            var info = MoveContainer(oldIndex, newIndex, item);
            RaiseRecycled(new ItemContainerEventArgs(info));

            return true;
        }
    }
}

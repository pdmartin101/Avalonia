using System;
using Avalonia.Collections;
using Avalonia.Controls.Presenters;
using Avalonia.Data;

namespace Avalonia.Controls.Generators
{
    /// <summary>
    /// Creates containers for items and maintains a list of created containers.
    /// </summary>
    /// <typeparam name="T">The type of the container.</typeparam>
    public class GroupContainerGenerator : ItemContainerGenerator
    {
        private ItemsControl _overallOwner;
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupContainerGenerator"/> class.
        /// </summary>
        /// <param name="owner">The immediate owner of the control.</param>
        /// <param name="overallOwner">The itemscontrol that owns all the groups.</param>
        public GroupContainerGenerator(IControl owner, ItemsControl overallOwner)
            : base(owner)
        {
            Contract.Requires<ArgumentNullException>(owner != null);
            _overallOwner = overallOwner;
        }

        /// <inheritdoc/>
        public override Type ContainerType => typeof(GroupItem);

        /// <inheritdoc/>
        protected override IControl CreateContainer(object item)
        {
            var container = item as GroupItem;

            if (container != null)
                return container;
            else
            {
                var itemsControl = Owner as ItemsControl;
                var presenter = itemsControl.Presenter as ItemsPresenter;
                var result = new GroupItem(itemsControl);
                result.SetValue(GroupItem.TemplatedParentProperty, Owner,BindingPriority.TemplatedParent);
                result.GroupParent = _overallOwner;
                result.Items = (GroupingViewInternal)item;
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

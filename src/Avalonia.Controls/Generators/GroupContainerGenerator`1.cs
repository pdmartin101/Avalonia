using System;
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
    public class GroupContainerGenerator<T> : ItemContainerGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemContainerGenerator{T}"/> class.
        /// </summary>
        /// <param name="owner">The owner control.</param>
        /// <param name="contentProperty">The container's Content property.</param>
        /// <param name="contentTemplateProperty">The container's ContentTemplate property.</param>
        public GroupContainerGenerator(
            IControl owner)
            : base(owner)
        {
            Contract.Requires<ArgumentNullException>(owner != null);
            //Contract.Requires<ArgumentNullException>(contentProperty != null);

            //ContentProperty = contentProperty;
            //ContentTemplateProperty = contentTemplateProperty;
        }

        /// <inheritdoc/>
        public override Type ContainerType => typeof(GroupItem);

        /// <summary>
        /// Gets the container's Content property.
        /// </summary>
        //protected AvaloniaProperty ContentProperty { get; }

        ///// <summary>
        ///// Gets the container's ContentTemplate property.
        ///// </summary>
        //protected AvaloniaProperty ContentTemplateProperty { get; }

        /// <inheritdoc/>
        protected override IControl CreateContainer(object item)
        {
            var container = item as GroupItem;

            if (container != null)
            {
                return container;
            }
            else
            {
                var result = new GroupItem();

                //if (ContentTemplateProperty != null)
                //{
                //    result.SetValue(ContentTemplateProperty, ItemTemplate, BindingPriority.Style);
                //}

                if (result is GroupItem)
                {
                    var itemsControl = Owner as ItemsControl;
//                    var icg = new ItemContainerGenerator<ListBoxItem>(Owner, ListBoxItem.ContentProperty, ListBoxItem.ContentTemplateProperty);
                    var icg = itemsControl.CreateLeafItemContainerGenerator();
                    result.SetValue(ItemsPresenter.TemplatedParentProperty, Owner,BindingPriority.TemplatedParent);
                    result.Items = ((GroupListItem)item).Items;
                    result.SetValue(GroupItem.ItemsPanelProperty, itemsControl.ItemsPanel);
                    result.ItemTemplate = itemsControl?.ItemTemplate;
                    //                    result.ItemContainerGenerator = icg;
                    //var itemsPresenter = new ItemsPresenter();
                    //itemsPresenter.SetValue(ItemsPresenter.TemplatedParentProperty, result,BindingPriority.TemplatedParent);
                    //itemsPresenter.ItemContainerGenerator = icg;
                    //itemsPresenter.Items = ((GroupListItem)item).Items;
                    //itemsPresenter.SetValue(ItemsPresenter.ItemsPanelProperty, itemsControl.ItemsPanel);
                    //itemsPresenter.VirtualizationMode= ItemVirtualizationMode.Smooth;
                    //itemsPresenter.ItemTemplate = itemsControl?.ItemTemplate;
                    icg.ItemTemplate = itemsControl?.ItemTemplate;
//                    result.SetPresenter(itemsPresenter);
//                    result.SetValue(result.PresenterProperty, itemsPresenter, BindingPriority.Style);
                }
                //else
                //    result.SetValue(ContentProperty, item, BindingPriority.Style);

                if (!(item is IControl))
                {
                    result.DataContext = item;
                }

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

//            container.SetValue(ContentProperty, item);

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

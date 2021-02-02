using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Utils;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Presenters
{
    internal class VirtualizedRealizedItems : IEnumerable<ItemContainerInfo>
    {
        private readonly IPanel _panel;
        private readonly ScrollViewer _scrollViewer;
        private readonly IEnumerable _items;
        private readonly IItemContainerGenerator _generator;
        private readonly ItemsPresenter _owner;
        private readonly bool _isItemScroll;
        private int _id;

        public int Count => _generator.Containers.Count();

        public VirtualizedRealizedItems(ItemsPresenter owner, int id)
        {
            _owner = owner;
            _panel = owner.Panel;
            _items = owner.Items;
            _generator = owner.ItemContainerGenerator;
            _scrollViewer = owner.FindAncestorOfType<ScrollViewer>();
            _isItemScroll = owner.VirtualizationMode == ItemVirtualizationMode.Logical;
            _id = id;
        }

        public IEnumerator<ItemContainerInfo> GetEnumerator()
        {
            return _generator.Containers.GetEnumerator();
        }

        public void AddChildren(VirtualizedRealizedItemsInfo info)
        {
            var numItems = _items.Count();
            info.SetPanelRelative(-_panel.TranslatePoint(new Point(0, 0), _scrollViewer).Value, _scrollViewer.Bounds.Size);
            if (_isItemScroll)
            {
                int scrollVal = (int)_scrollViewer.Offset.Y;
                if (_items is IGroupingView gv)
                    scrollVal=gv.GetItemPosition((int)_scrollViewer.Offset.Y);
                info.SetFirst(_items,scrollVal);
            }
            else
                info.SetFirst(_items);
            var count = 0;
//            System.Console.WriteLine($"BeforeAdd {this} {info._currentOffset} {info.PanelOffset}");
            while (info.RealizeNeeded(numItems))
            {
                info.AddOffset(AddOneChild(info));
//                System.Console.WriteLine($"Add Item {this} {count} {info._currentOffset}");
                count++;
            }
            System.Console.WriteLine($"AddChildren Realized {this} {count} Info {info}  Scroller Height {_scrollViewer.Bounds.Height}");
        }

        public void RemoveChildren(VirtualizedRealizedItemsInfo info)
        {
            var toRemove = new List<int>();
            foreach (var item in _generator.Containers)
            {
                if (info.CheckForRemoval(item.Index))
                    toRemove.Add(item.Index);
            }
            if (toRemove.Count != 0)
                System.Console.WriteLine($"RemoveChildren Realized {this}  Info {info}  Scroller Height {_scrollViewer.Bounds.Height}");
            foreach (var toRem in toRemove)
                foreach (var container in _generator.Dematerialize(toRem, 1))
                    _panel.Children.Remove(container.ContainerControl);
        }

        internal double AddOneChild(VirtualizedRealizedItemsInfo info)
        {
            var child = _generator.ContainerFromIndex(info.Next);
            if (child == null)
            {
                var materialized = _generator.Materialize(info.Next, _items.ElementAt(info.Next));
                child = materialized.ContainerControl;
                _panel.Children.Add(child);
                child.Measure(Size.Infinity);
//                System.Console.WriteLine($"Add Item00 {this} {info._currentOffset}");
                if (VirtualizingAverages.AddContainerSize(_panel.TemplatedParent, _items.ElementAt(info.Next), child))
                    _owner.InvalidateMeasure();
            }
            else
            {
                if (child is GroupItem gi)
                {
                    gi.Presenter?.InvalidateMeasure();
                }
                child.Measure(Size.Infinity);
//                System.Console.WriteLine($"Add Item01 {this} {info._currentOffset}");
                if (VirtualizingAverages.AddContainerSize(_panel.TemplatedParent, _items.ElementAt(info.Next), child))
                    _owner.InvalidateMeasure();
            }
            return info.Vert? child.DesiredSize.Height:child.DesiredSize.Width;
        }
        public IControl ContainerFromIndex(int indx)
        {
            return _generator.ContainerFromIndex(indx);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        ~VirtualizedRealizedItems()
        {
            //            Logging.Virtual.LogWarning($"Finalized Realized {--_count}");
        }

        public override string ToString()
        {
            return $"{_id}  Items:{_items}   Generator:{_generator.Containers.Count()}";
        }
    }
}

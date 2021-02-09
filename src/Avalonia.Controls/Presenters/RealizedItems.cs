using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Utils;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Presenters
{
    internal class RealizedItems : IEnumerable<ItemContainerInfo>
    {
        private readonly IPanel _panel;
        private readonly ScrollViewer _scrollViewer;
        private readonly IEnumerable _items;
        private ItemVirtualizingCache _cache;
        private readonly IItemContainerGenerator _generator;
        private readonly ItemsPresenter _owner;
        private GroupController _groupControl;
        private int _id;
        public RealizedItemsInfo _info { get; set; }

        public int Count => _generator.Containers.Count();

        public RealizedItems(ItemsPresenter owner, GroupController groupControl, int id)
        {
            _owner = owner;
            _panel = owner.Panel;
            _items = owner.Items;
            _generator = owner.ItemContainerGenerator;
            _scrollViewer = owner.FindAncestorOfType<ScrollViewer>();
            _id = id;
            _cache = owner.VirtualizingCache;
            _groupControl = groupControl;
            if (owner.VirtualizationMode == ItemVirtualizationMode.Logical)
                _info = new RealizedItemsInfo2(_items, groupControl.Vert, _cache, groupControl.TemplatedParent);
            else
                _info = new RealizedItemsInfo(_items, groupControl.Vert, _cache, groupControl.TemplatedParent);
        }

        public IEnumerator<ItemContainerInfo> GetEnumerator()
        {
            return _generator.Containers.GetEnumerator();
        }

        public void AddChildren()
        {
//            _groupControl.RemoveGroup(_items);
            var numItems = _items.Count();
            _info.SetPanelRelative(-_panel.TranslatePoint(new Point(0, 0), _scrollViewer).Value, _scrollViewer.Bounds.Size);
            _info.SetFirst(_scrollViewer.Offset);
            //            System.Console.WriteLine($"BeforeAdd {this} {Info._currentOffset} {Info.PanelOffset}");
            while (_info.RealizeNeeded(numItems))
            {
                _info.AddOffset(AddOneChild(_info));
                //                System.Console.WriteLine($"Add Item {this} {count} {Info._currentOffset}");
            }
            System.Console.WriteLine($"AddChildren Realized {this} Info {_info}  Scroller Height {_scrollViewer.Bounds.Height}");
//            _groupControl.AddGroup(_info, _items, _generator);
        }

        public void RemoveChildren()
        {
            var toRemove = new List<ItemContainerInfo>();
            foreach (var item in _generator.Containers)
            {
                if (_info.CheckForRemoval(item.Index))
                    toRemove.Add(item);
            }
            if (toRemove.Count != 0)
                System.Console.WriteLine($"RemoveChildren Realized {this}  Info {_info}  Scroller Height {_scrollViewer.Bounds.Height}");
            foreach (var toRem in toRemove)
            {
                foreach (var container in _generator.Dematerialize(toRem.Index, 1))
                    _panel.Children.Remove(container.ContainerControl);
                //                _groupControl.RemoveGroup(toRem.Item);
            }
        }

        internal double AddOneChild(RealizedItemsInfo info)
        {
            var child = _generator.ContainerFromIndex(_info.Next);
            if (child == null)
            {
                var materialized = _generator.Materialize(_info.Next, _items.ElementAt(_info.Next));
                child = materialized.ContainerControl;
                _panel.Children.Add(child);
                child.Measure(Size.Infinity);
                //                System.Console.WriteLine($"Add Item00 {this} {Info._currentOffset}");
                if (VirtualizingAverages.AddContainerSize(_groupControl.TemplatedParent, _items.ElementAt(_info.Next), child))
                    _owner.InvalidateMeasure();
            }
            else
            {
                if (child is GroupItem gi)
                {
                    gi.Presenter?.InvalidateMeasure();
                }
                if (_id==402)
                { }
                child.Measure(Size.Infinity);
                //                System.Console.WriteLine($"Add Item01 {this} {Info._currentOffset}");
                if (VirtualizingAverages.AddContainerSize(_groupControl.TemplatedParent, _items.ElementAt(_info.Next), child))
                    _owner.InvalidateMeasure();
            }
            return _info.Vert ? child.DesiredSize.Height : child.DesiredSize.Width;
        }
        public IControl ContainerFromIndex(int indx)
        {
            return _generator.ContainerFromIndex(indx);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        ~RealizedItems()
        {
            //            Logging.Virtual.LogWarning($"Finalized Realized {--_count}");
        }

        public override string ToString()
        {
            return $"{_id}  Items:{_items}   Generator:{_generator.Containers.Count()}";
        }
    }
    public class GroupController
    {
        public ItemsControl TemplatedParent { get; set; }
        public IControl Panel { get; set; }
        public int LastInFullView { get; set; }
        public IGroupingView AllItems { get; set; }
        public bool Vert { get; set; }
        //        public int NumInView { get; set; }
        private Dictionary<int, RealizedItemInfo> RealizedIndex { get; set; } = new Dictionary<int, RealizedItemInfo>();
        private Dictionary<object, RealizedLookup> RealizedLookups { get; set; } = new Dictionary<object, RealizedLookup>();
        private int _oldLastInFullView = -1;

        internal bool Changed()
        {
            if (LastInFullView != _oldLastInFullView)
            {
                _oldLastInFullView = LastInFullView;
                return true;
            }
            return false;
        }
        internal void RemoveGroup(object vm)
        {
            System.Console.WriteLine($"Rem00 Group {vm}");
            if (!RealizedLookups.TryGetValue(vm, out var lookups))
            {
                lookups = new RealizedLookup();
                RealizedLookups.Add(vm, lookups);
                //                System.Console.WriteLine($"Rem01 Group {vm}");
            }
            foreach (var item in lookups.Indexes)
            {
                RealizedIndex.Remove(item);
            }
            lookups.Indexes.Clear();
            lookups.LastInFullView = -1;
        }
        internal void AddGroup(RealizedItemsInfo info, object vm, IItemContainerGenerator generator)
        {
            System.Console.WriteLine($"Add00 Group {vm} {generator.Id}");
            if (!RealizedLookups.TryGetValue(vm, out var indexList))
            {
                indexList = new RealizedLookup();
                RealizedLookups.Add(vm, indexList);
                //                System.Console.WriteLine($"Add01 Group {vm} {generator.Id}");
            }
            foreach (var item in generator.Containers)
            {
                var res = item.ContainerControl.TranslatePoint(new Point(0, 0), Panel);
                var realizedInfo = new RealizedItemInfo() { Container = item, RelativeOffset = res.Value.Y };
                if (item.ContainerControl.Bounds.IsEmpty)
                    System.Console.WriteLine($"Empty Container {item.Index} {info}");


                    if (item.Item is IGroupingView gv)
                {
                    System.Console.WriteLine($"Adding Group to Groups {generator.Id} {item.Item} {gv.ItemScrollStart}");
                    RealizedIndex.Add(gv.ItemScrollStart, realizedInfo);
                    indexList.Indexes.Add(gv.ItemScrollStart);
                    if (item.Index <= info.NumInFullView)
                        indexList.LastInFullView = gv.ItemScrollStart;
                }
                else
                {
                    var groupsPos = item.Index + info.ScrollOffset;
                    System.Console.WriteLine($"Adding Item to Groups {generator.Id} {item.Item}  {info.ScrollOffset}");
                    RealizedIndex.Add(groupsPos, realizedInfo);
                    indexList.Indexes.Add(groupsPos);
                    if (item.Index < info.NumInFullView)
                        indexList.LastInFullView = groupsPos;
                    //                   startDelta++;
                }
            }
            var last = -1;
            foreach (var lookup in RealizedLookups)
            {
                if (lookup.Value.LastInFullView > last)
                    last = lookup.Value.LastInFullView;
            }
            LastInFullView = last;
            System.Console.WriteLine($"LastInView {last}");
        }

        internal bool GetItemByIndex(int indx, out ItemContainerInfo container)
        {
            RealizedItemInfo realizedInfo;
            if (RealizedIndex.TryGetValue(indx, out realizedInfo))
            {
                container = realizedInfo.Container;
                return true;
            }
            container = null;
            return false;
        }

        public int GetNumInView(double scrollPos)
        {
            var last = (int)scrollPos;
            var offset = 0.0;
            if (RealizedIndex.TryGetValue((int)scrollPos, out var first))
            {
                var firstLocation = first.RelativeOffset;
                //               var res = first.ContainerControl.TranslatePoint(new Point(0, 0), Panel);
                //               while ((indx < RealizedIndex.Count) && (offset <= 248.8)) //Panel.DesiredSize.Height))
                foreach (var item in RealizedIndex)
                //                    if (RealizedIndex.TryGetValue(indx, out var item))
                {
                    offset = item.Value.RelativeOffset - firstLocation;
                    var res = item.Value.Container.ContainerControl.TranslatePoint(new Point(0, 0), Panel);
                    if (item.Value.Container.ContainerControl.Bounds.IsEmpty)
                        System.Console.WriteLine($"Empty Container02 {item.Value.Container.Index}");

                    if ((offset <= 248.8) && (last < item.Key))// Panel.DesiredSize.Height)
                    {
                        if (offset + item.Value.Container.ContainerControl.DesiredSize.Height <= 248.8)
                            last = item.Key;
                        else
                            last = item.Key - 1;
                    }
                }
            }
            System.Console.WriteLine($"LastInView02 {last}");
            return last + 1 - (int)scrollPos;
        }
        class RealizedLookup
        {
            public List<int> Indexes { get; set; } = new List<int>();
            public int LastInFullView { get; set; }
            public double RelativeOffset { get; set; }
        }
        class RealizedItemInfo
        {
            public ItemContainerInfo Container { get; set; }
            public double RelativeOffset { get; set; }
        }
    }

}

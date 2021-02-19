using System;
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
                _info = new RealizedItemsInfo2(_items, groupControl.Vertical, _cache, groupControl.TemplatedParent);
            else
                _info = new RealizedItemsInfo(_items, groupControl.Vertical, _cache, groupControl.TemplatedParent);
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
            PdmLogger.Log(3, PdmLogger.IndentEnum.Nothing, $"BeforeAdd {this} {_info}");
            while (_info.RealizeNeeded(numItems))
            {
                _info.AddOffset(AddOneChild(_info));
                PdmLogger.Log(3, PdmLogger.IndentEnum.Nothing, $"Add Item {this} {_info}");
            }
            PdmLogger.Log(2, PdmLogger.IndentEnum.Nothing, $"AddChildren Realized {this} Info {_info}  Scroller Height {_scrollViewer.Bounds.Height:0.00}");
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
                PdmLogger.Log(30, PdmLogger.IndentEnum.Nothing, $"RemoveChildren {this}  Info {_info} {toRemove.Count}");
            foreach (var toRem in toRemove)
            {
                foreach (var container in _generator.Dematerialize(toRem.Index, 1))
                {
                    PdmLogger.Log(30, PdmLogger.IndentEnum.Nothing, $"Remove Child {container.ContainerControl}");
                    _panel.Children.Remove(container.ContainerControl);
                    _groupControl.RemoveItem(container.ContainerControl);
                }
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
                PdmLogger.Log(2, PdmLogger.IndentEnum.Nothing, $"Add new Item {child.DataContext} to {this} {_info._currentOffset:0.00}");
                if (VirtualizingAverages.AddContainerSize(_groupControl.TemplatedParent, _items.ElementAt(_info.Next), child))
                    _owner.InvalidateMeasure();
            }
            else
            {
                if (child is GroupItem gi)
                {
                    gi.Presenter?.InvalidateMeasure();
                }
                if (_id == 402)
                { }
                child.Measure(Size.Infinity);
                PdmLogger.Log(2, PdmLogger.IndentEnum.Nothing, $"Add old Item {child.DataContext} to {this} {_info._currentOffset:0.00}");
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
        public ItemsControl TemplatedParent { get; private set; }
        private ScrollContentPresenter _scrollContentPresenter;
        private IControl _panel;
        public int NumInFullView { get; private set; }
        public bool Vertical { get; private set; }
        private Dictionary<int, RealizedItemInfo> RealizedIndex { get; set; } = new Dictionary<int, RealizedItemInfo>();

        public GroupController(ItemsControl itemsControl, bool vert)
        {
            TemplatedParent = itemsControl;
            Vertical = vert;
            _scrollContentPresenter = itemsControl.FindDescendantOfType<ScrollContentPresenter>();
            _panel = itemsControl.FindDescendantOfType<IVirtualizingPanel>();
        }
        private void RemoveOld()
        {
//            PdmLogger.Log(3, PdmLogger.IndentEnum.Nothing, $"Rem Group");
            var toRemove = new List<int>();
            foreach (var item in RealizedIndex)
            {
                if ((item.Value.Container == null) || (item.Value.Container.ContainerControl.Parent == null))
                    toRemove.Add(item.Key);
            }
            foreach (var item in toRemove)
            {
                PdmLogger.Log(3, PdmLogger.IndentEnum.Nothing, $"Rem Item02 {RealizedIndex[item].Container.ContainerControl.DataContext}");

                RealizedIndex.Remove(item);
            }
        }
        public void RemoveItem(IControl control)
        {
            PdmLogger.Log(3, PdmLogger.IndentEnum.Nothing, $"Rem Item");
            var toRemove = new List<int>();
            foreach (var item in RealizedIndex)
            {
                if (item.Value.Container.ContainerControl == control)
                    toRemove.Add(item.Key);
            }
            foreach (var item in toRemove)
            {
                PdmLogger.Log(3, PdmLogger.IndentEnum.Nothing, $"Rem Item00 {RealizedIndex[item].Container.ContainerControl.DataContext}");

                RealizedIndex.Remove(item);
            }
        }
       
        internal void RemoveItems(IItemContainerGenerator itemContainerGenerator)
        {
            var toRemove = new List<int>();
            foreach (var container in itemContainerGenerator.Containers)
            {
                foreach (var item in RealizedIndex)
                {
                    if (item.Value.Container == container)
                        toRemove.Add(item.Key);
                }
            }
            foreach (var item in toRemove)
            {
                PdmLogger.Log(3, PdmLogger.IndentEnum.Nothing, $"Rem Item01 {RealizedIndex[item].Container.ContainerControl.DataContext}");

                RealizedIndex.Remove(item);
            }
        }

        internal void AddGroup(RealizedItemsInfo info, IItemContainerGenerator generator, int scrollPos)
        {
            var previousNumInfullView = NumInFullView;
            RemoveOld();
            foreach (var item in generator.Containers)
            {
                var res = item.ContainerControl.TranslatePoint(new Point(0, 0), _panel);
                var itemPos = (item.Item is IGroupingView gv) ? gv.ItemScrollStart : item.Index + info.ScrollOffset;
                if (!RealizedIndex.TryGetValue(itemPos, out var realizedInfo))
                {
                    realizedInfo = new RealizedItemInfo();
                    RealizedIndex.Add(itemPos, realizedInfo);
                }
                realizedInfo.Set(item);
                realizedInfo.RelativeOffset = res.Value.Y;
            }
            NumInFullView = GetNumInView(scrollPos);
            if (NumInFullView == 8)
            { }
            if (previousNumInfullView != NumInFullView)
                _scrollContentPresenter.InvalidateArrange();
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

        private int GetNumInView(double scrollPos)
        {
            var last = (int)scrollPos;
            RemoveOld();
            if (RealizedIndex.TryGetValue((int)scrollPos, out var first))
            {
                var firstLocation = first.RelativeOffset;
                foreach (var item in RealizedIndex)
                {
                    var offset = item.Value.RelativeOffset - firstLocation;
                    var res = item.Value.Container.ContainerControl.TranslatePoint(new Point(0, 0), _panel);
                    if ((offset <= _scrollContentPresenter.DesiredSize.Height) && (last < item.Key))// Panel.DesiredSize.Height)
                    {
                        if (offset + item.Value.Container.ContainerControl.DesiredSize.Height <= _scrollContentPresenter.DesiredSize.Height)
                            last = item.Key;
                        else
                            last = item.Key - 1;
                    }
                }
            }
            PdmLogger.Log(3, PdmLogger.IndentEnum.Nothing, $"LastInView02 {scrollPos} {last}");
            return last + 1 - (int)scrollPos;
        }
        //class RealizedLookup
        //{
        //    public List<int> Indexes { get; set; } = new List<int>();
        //    public int LastInFullView { get; set; }
        //    public double RelativeOffset { get; set; }
        //}
        class RealizedItemInfo
        {
            public WeakReference<ItemContainerInfo> _container { get; set; }
            public ItemContainerInfo Container => GetContainer();

            public void Set(ItemContainerInfo container)
            {
                _container = new WeakReference<ItemContainerInfo>(container);
            }
            private ItemContainerInfo GetContainer()
            {
                _container.TryGetTarget(out var container);
                return container;
            }

            public double RelativeOffset { get; set; }
        }

    }

}

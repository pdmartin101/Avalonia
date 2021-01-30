using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Utils;
using Avalonia.Styling;
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
        public static int _count;


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

        public void AddChildren(RealizedChildrenInfo info)
        {
            var numItems = _items.Count();
            info.SetPanelRelative(-_panel.TranslatePoint(new Point(0, 0), _scrollViewer).Value, _scrollViewer.Bounds.Size);
            if (_isItemScroll)
            {
                int scrollVal = (int)_scrollViewer.Offset.Y;
                if (_items is IGroupingView gv)
                    scrollVal=gv.GetItemPosition((int)_scrollViewer.Offset.Y);
                info.SetFirst(_panel.TemplatedParent, _items,scrollVal);
            }
            else
                info.SetFirst(_panel.TemplatedParent, _items);
            var count = 0;
//            System.Console.WriteLine($"BeforeAdd {this} {info._currentOffset} {info.PanelOffset}");
            while (info.Realize(numItems))
            {
                info.AddOffset(AddOneChild(info));
//                System.Console.WriteLine($"Add Item {this} {count} {info._currentOffset}");
                count++;
            }
            if (_generator.Containers.Count()>100)
            { }
            System.Console.WriteLine($"AddChildren Realized {this} {count} Info {info}  Scroller Height {_scrollViewer.Bounds.Height}");
        }

        public void RemoveChildren(RealizedChildrenInfo info)
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

        internal double AddOneChild(RealizedChildrenInfo info)
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

    class RealizedChildrenInfo
    {
        public double PanelOffset { get; private set; }
        public double HiOffset { get; private set; }
        public int FirstInView { get; private set; }
        public int LastInView => FirstInView + NumInView-1;
        public int LastInFullView => FirstInView + NumInFullView-1;
        public int NumInView { get; private set; }
        public int NumInFullView { get; private set; }

        internal double _currentOffset;
        public bool Vert { get;}
        public int Next => FirstInView + NumInView;

        public RealizedChildrenInfo(bool vert)
        {
            Vert = vert;
        }
        public void SetFirst( ITemplatedControl templatedParent, IEnumerable items, int first)
        {
            FirstInView = first<0?0:first;
            NumInView = 0;
            NumInFullView = 0;
            _currentOffset = VirtualizingAverages.GetOffsetForIndex(templatedParent, FirstInView, items, Vert);
        }

        internal void AddOffset(double offset)
        {
            _currentOffset += offset;
            if (_currentOffset <= HiOffset)
                NumInFullView++;
            NumInView++;
        }

        internal void SetPanelRelative(Point relPos, Size size)
        {
            PanelOffset = Vert ? relPos.Y : relPos.X;
            _currentOffset = PanelOffset;
            HiOffset = PanelOffset + (Vert ? size.Height : size.Width);
        }

        internal bool Realize(int numItems)
        {
            return _currentOffset < HiOffset && Next < numItems;
        }

        internal void SetFirst(ITemplatedControl templatedParent, IEnumerable items)
        {
            FirstInView = VirtualizingAverages.GetStartIndex(templatedParent, PanelOffset, items, Vert);
            _currentOffset = VirtualizingAverages.GetOffsetForIndex(templatedParent, FirstInView, items, Vert);
            NumInView = 0;
            NumInFullView = 0;
        }

        internal bool CheckForRemoval(int indx)
        {
            if ((indx < FirstInView) || (indx > LastInView))
                return true;
            return false;
        }

        public override string ToString()
        {
            return $"{PanelOffset}:{HiOffset}  {FirstInView}:{LastInView}";
        }
    }
}

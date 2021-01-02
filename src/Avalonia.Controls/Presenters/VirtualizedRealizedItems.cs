using System;
using System.Collections;
using System.Collections.Generic;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Utils;

namespace Avalonia.Controls.Presenters
{
    internal class VirtualizedRealizedItems : IEnumerable<ItemContainerInfo>
    {
        private readonly IVirtualizingPanel _panel;
        private readonly ScrollViewer _scrollViewer;
        private readonly IEnumerable _items;
        private readonly IItemContainerGenerator _generator;
        private bool _measureRequired;
        private int _id;
        public static int _count;

        public int Count => _generator.Containers.Count();

        public VirtualizedRealizedItems(IVirtualizingPanel panel,ScrollViewer scrollViewer, IEnumerable items, IItemContainerGenerator generator,int id)
        {
            _panel = panel;
            _items = items;
            _generator = generator;
            _scrollViewer = scrollViewer;
            _id=id;
        }

        public IEnumerator<ItemContainerInfo> GetEnumerator()
        {
            return _generator.Containers.GetEnumerator();
        }

        public RealizedChildrenInfo AddChildren(bool vert)
        {
            _measureRequired = false;
            var numItems = _items.Count();
            var rel = -_panel.TranslatePoint(new Point(0, 0), _scrollViewer);
            var rel2 = _scrollViewer.Offset;
            var panelOffset = vert ? rel.Value.Y: rel.Value.X;
            var hiOffset = panelOffset + (vert?_scrollViewer.Bounds.Height: _scrollViewer.Bounds.Width);
            var first = VirtualizingAverages.GetStartIndex(_panel.TemplatedParent, panelOffset, _items,vert);
            var indx = first;
            var offset = VirtualizingAverages.GetOffsetForIndex(_panel.TemplatedParent, indx, _items,vert);
            var extra = offset - panelOffset;
//            System.Console.WriteLine($"AddChildren {_id} {rel} {first} {offset} {_scrollViewer.Bounds.Height}");
            while ((offset < hiOffset) && (indx < numItems))
                offset += AddOneChild(indx++);
            return new RealizedChildrenInfo { FirstInView = first, LastInView = indx - 1, LastInFullView = offset <= hiOffset ? indx - 1 : indx - 2, RequiresReMeasure=_measureRequired, Margin=4};
        }

        public void RemoveChildren(bool vert)
        {
            var rel = -_panel.TranslatePoint(new Point(0, 0), _scrollViewer);
            var offset = vert ? rel.Value.Y : rel.Value.X;
            var hiOffset = offset + (vert ? _scrollViewer.Bounds.Height : _scrollViewer.Bounds.Width);
            var startIndx = VirtualizingAverages.GetStartIndex(_panel.TemplatedParent, offset, _items,vert);
            var endIndx = VirtualizingAverages.GetStartIndex(_panel.TemplatedParent, hiOffset, _items,vert);
            var toRemove = new List<int>();
            foreach (var item in _generator.Containers)
            {
                if ((item.Index < startIndx) || (item.Index > endIndx))
                    toRemove.Add(item.Index);
            }
            foreach (var toRem in toRemove)
                foreach (var container in _generator.Dematerialize(toRem, 1))
                    _panel.Children.Remove(container.ContainerControl);
        }
        internal double AddOneChild(int indx)
        {
             var child = _generator.ContainerFromIndex(indx);
            if (child == null)
            {
                var materialized = _generator.Materialize(indx, _items.ElementAt(indx));
                child = materialized.ContainerControl;
                _panel.Children.Add(child);
                child.Measure(Size.Infinity);
 //               System.Console.WriteLine($"AddOneChild {_id} {indx} {_items.ElementAt(indx)} {child.DesiredSize}");
            }
            else
            {
                child.Measure(Size.Infinity);
//                System.Console.WriteLine($"AddOneChild2 {_id} {indx} {_items.ElementAt(indx)} {child.DesiredSize}");
            }
            var diff = VirtualizingAverages.AddContainerSize(_panel.TemplatedParent, _items.ElementAt(indx), child.DesiredSize);
            if (diff != Size.Empty)
                _measureRequired = true;
            return child.DesiredSize.Height;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        ~VirtualizedRealizedItems()
        {
            //            Logging.Virtual.LogWarning($"Finalized Realized {--_count}");
        }
    }

    class RealizedChildrenInfo
    {
        public int FirstInView { get; set; }
        public int LastInView { get; set; }
        public int LastInFullView { get; set; }
        public double Margin { get; set; }
        public bool RequiresReMeasure { get; set; }

        public int NumInFullView => LastInFullView - FirstInView+1;
        public void SetFirst(double first)
        {
            FirstInView = (int)first;
        }
    }
}

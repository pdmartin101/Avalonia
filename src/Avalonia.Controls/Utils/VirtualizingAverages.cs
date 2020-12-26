using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Avalonia.Styling;

namespace Avalonia.Controls.Utils
{
    public static class VirtualizingAverages
    {
        private static Dictionary<ITemplatedControl, VirtualizingSizes> _controls = new Dictionary<ITemplatedControl, VirtualizingSizes>();

        public static Size AddContainerSize(ITemplatedControl control, object item, Size size)
        {
            VirtualizingSizes sizes;
            //            System.Console.WriteLine($"VirtualizingControls:AddContainerSize {item}, {size}");
            var templatedParent = GetTopTemplatedParent(control);
            if (!_controls.TryGetValue(templatedParent, out sizes))
            {
                sizes = new VirtualizingSizes();
                _controls.Add(templatedParent, sizes);
            }
            return sizes.AddContainerSize(item, size);
        }
 
        private static bool GetContainerSize(ITemplatedControl control, object item,bool vert, out double size)
        {
            VirtualizingSizes sizes;
            var templatedParent = GetTopTemplatedParent(control);
            size = 0;
            if (!_controls.TryGetValue(templatedParent, out sizes))
                return false;
            var ret = sizes.GetContainerSize(item, out var size2);
            size = vert ? size2.Height : size2.Width;
            return ret;
        }

        public static double GetEstimatedExtent(ITemplatedControl control, IEnumerable items, bool vert)
        {
            var templatedParent = GetTopTemplatedParent(control);
            return GetEstimatedAverage(templatedParent, items, vert) * items.Count();
        }

        public static double GetEstimatedAverage(ITemplatedControl control, IEnumerable items, bool vert)
        {
            var totalKnown = 0.0;
            var countKnown = 0;
            var countItems = 0;
            var templatedParent = GetTopTemplatedParent(control);
            foreach (var item in items)
            {
                countItems++;
                if (GetContainerSize(templatedParent, item, vert, out var size))
                {
                    countKnown++;
                    totalKnown += size;
                }
            }
            return countKnown > 0 ? (totalKnown / countKnown) : 0;
        }

        public static int GetStartIndex(ITemplatedControl control, double offset, IEnumerable items, bool vert)
        {
            var templatedParent = GetTopTemplatedParent(control);
            if (!_controls.TryGetValue(templatedParent, out var sizes))
                return 0;
            return sizes.GetStartIndex(offset,items, vert);
        }
        public static double GetOffsetForIndex(ITemplatedControl control, int indx, IEnumerable items, bool vert)
        {
            var templatedParent = GetTopTemplatedParent(control);
            if (!_controls.TryGetValue(templatedParent, out var sizes))
                return 0;
            return sizes.GetOffsetForIndex(indx,items, vert);
        }
        private static ITemplatedControl GetTopTemplatedParent(ITemplatedControl templatedParent)
        {
            var topParent = templatedParent;
            while (topParent is GroupItem g)
                topParent = g.TemplatedParent;
            return topParent;
        }
    }
    public class VirtualizingSizes
    {
        private Dictionary<object, Size> _containers = new Dictionary<object, Size>();
        private double _vTotal = 0.0;
        private double _hTotal = 0.0;
        public double VertAverage => _containers.Count == 0 ? 0.0 : _vTotal / _containers.Count;
        public double HorizAverage => _containers.Count == 0 ? 0.0 : _hTotal / _containers.Count;
        public Size AddContainerSize(object item, Size size)
        {
            var savedSize=Size.Empty;
            if (_containers.TryGetValue(item, out savedSize))
            {
                _containers.Remove(item);
                _vTotal -= savedSize.Height;
                _hTotal -= savedSize.Width;
            }
            _containers.Add(item, size);
            _vTotal += size.Height;
            _hTotal += size.Width;
            return size - savedSize;
        }
        public bool GetContainerSize(object item, out Size size)
        {
            if (_containers.TryGetValue(item, out size))
                return true;
            return false;
        }
        public int GetStartIndex(double offset, IEnumerable items, bool vert)
        {
            var currentPos = 0.0;
            var startIndx = 0;
            foreach (var item in items)
            {
                if (_containers.TryGetValue(item, out var size))
                    currentPos += vert ? size.Height : size.Width;
                else
                    currentPos += vert ? VertAverage : HorizAverage;
                if (currentPos > offset)
                    break;
                startIndx++;
            }
            return startIndx;
        }

        public double GetOffsetForIndex(int indx, IEnumerable items, bool vert)
        {
            var currentPos = 0.0;
            var startIndx = 0;
            foreach (var item in items)
            {
                if (startIndx == indx)
                    break;
                if (_containers.TryGetValue(item, out var size))
                    currentPos += vert?size.Height:size.Width;
                else
                    currentPos += vert ? VertAverage : HorizAverage;
                startIndx++;
            }
            return currentPos;
        }

    }
}

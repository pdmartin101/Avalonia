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

        public static bool AddContainerSize(ITemplatedControl templatedControl, object vmItem, IControl control)
        {
            VirtualizingSizes sizes;
            //            System.Console.WriteLine($"VirtualizingControls:AddContainerSize {item}, {size}");
            var templatedParent = GetTopTemplatedParent(templatedControl);
            if (!_controls.TryGetValue(templatedParent, out sizes))
            {
                sizes = new VirtualizingSizes();
                _controls.Add(templatedParent, sizes);
            }
            return sizes.AddContainerSize(vmItem, control);
        }
 
        private static bool GetContainerSize(ITemplatedControl control, object item, out ContainerInfo containerInfo)
        {
            VirtualizingSizes sizes;
            var templatedParent = GetTopTemplatedParent(control);
            containerInfo = null;
            if (!_controls.TryGetValue(templatedParent, out sizes))
                return false;
            var ret = sizes.GetContainerSize(item, out containerInfo);
            return ret;
        }

        public static IControl GetControlForItem(ITemplatedControl control, object item)
        {
            var templatedParent = GetTopTemplatedParent(control);
            if (!_controls.TryGetValue(templatedParent, out var sizes))
                return null;
            return sizes.GetControlForItem(item);
        }

        public static Size GetEstimatedExtent(ITemplatedControl control, IEnumerable items, bool vert)
        {
            var templatedParent = GetTopTemplatedParent(control);
            var av = GetEstimatedAverage(templatedParent, items, vert);
            return vert ? av.WithHeight(av.Height * items.Count()) : av.WithWidth(av.Width * items.Count());
        }

        private static Size GetEstimatedAverage(ITemplatedControl control, IEnumerable items, bool vert)
        {
            var totalKnown = 0.0;
            var largestOther = 0.0;
            var countKnown = 0;
            var countItems = 0;
            var templatedParent = GetTopTemplatedParent(control);
            foreach (var item in items)
            {
                countItems++;
                if (GetContainerSize(templatedParent, item,  out var containerInfo))
                {
                    countKnown++;
                    if (vert)
                    {
                        totalKnown += containerInfo.ContainerSize.Height;
                        if (containerInfo.ContainerSize.Width > largestOther)
                            largestOther = containerInfo.ContainerSize.Width;
                    }
                    else
                    {
                        totalKnown += containerInfo.ContainerSize.Width;
                        if (containerInfo.ContainerSize.Height > largestOther)
                            largestOther = containerInfo.ContainerSize.Height;
                    }
                }
            }
            if (countKnown > 0)
            {
                if (vert)
                    return new Size(largestOther, totalKnown / countKnown);
                else
                    return new Size(totalKnown / countKnown, largestOther);
            }
            return new Size();
        }

        public static int GetStartIndex(ITemplatedControl control, double offset, IEnumerable items, bool vert)
        {
            var templatedParent = GetTopTemplatedParent(control);
            if (!_controls.TryGetValue(templatedParent, out var sizes))
                return 0;
            return sizes.GetStartIndex(offset, items, vert);
        }
        public static int GetEndIndex(ITemplatedControl control, double startIndx, Size size, IEnumerable items, bool vert)
        {
            var templatedParent = GetTopTemplatedParent(control);
            if (!_controls.TryGetValue(templatedParent, out var sizes))
                return 0;
            var offset = 0.0;
            var indx = (int)startIndx+1;
            var startOffset= sizes.GetOffsetForIndex((int)startIndx, items, vert);
            while ((offset < (vert?size.Height:size.Width)) && (indx < items.Count()))
            {
                offset= sizes.GetOffsetForIndex(indx, items, vert)-startOffset;
                indx++;
            }
            return indx;
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
        private Dictionary<object, ContainerInfo> _containers = new Dictionary<object, ContainerInfo>();
        private double _vTotal = 0.0;
        private double _hTotal = 0.0;
        public double VertAverage => _containers.Count == 0 ? 0.0 : _vTotal / _containers.Count;
        public double HorizAverage => _containers.Count == 0 ? 0.0 : _hTotal / _containers.Count;
        public bool AddContainerSize(object item, IControl control)
        {
            if (_containers.TryGetValue(item, out var savedInfo))
            {
                _containers.Remove(item);
                _vTotal -= savedInfo.ContainerSize.Height;
                _hTotal -= savedInfo.ContainerSize.Width;
            }
            var container = new ContainerInfo(control);
            _containers.Add(item, container);
            _vTotal += container.ContainerSize.Height;
            _hTotal += container.ContainerSize.Width;
            return (savedInfo==null) || (savedInfo.ContainerSize != container.ContainerSize);
        }
        internal bool GetContainerSize(object item, out ContainerInfo containerInfo)
        {
            if (_containers.TryGetValue(item, out containerInfo))
                return true;
            return false;
        }
        public int GetStartIndex(double offset, IEnumerable items, bool vert)
        {
            var currentPos = 0.0;
            var startIndx = 0;
            foreach (var item in items)
            {
                if (_containers.TryGetValue(item, out var containerInfo))
                    currentPos += vert ? containerInfo.ContainerSize.Height : containerInfo.ContainerSize.Width;
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
                if (_containers.TryGetValue(item, out var containerInfo))
                    currentPos += vert? containerInfo.ContainerSize.Height: containerInfo.ContainerSize.Width;
                else
                    currentPos += vert ? VertAverage : HorizAverage;
                startIndx++;
            }
            return currentPos;
        }

        internal IControl GetControlForItem(object item)
        {
            if (_containers.TryGetValue(item, out var containerInfo))
                if (containerInfo.Container.TryGetTarget(out var control))
                    return control;
            return null;
        }
    }

    class ContainerInfo
    {
        public WeakReference<IControl> Container { get; }
        public Size ContainerSize { get; }    // this can out live Container

        public ContainerInfo(IControl control)
        {
            Container = new WeakReference<IControl>(control);
            ContainerSize = control.DesiredSize;
        }
    }
}

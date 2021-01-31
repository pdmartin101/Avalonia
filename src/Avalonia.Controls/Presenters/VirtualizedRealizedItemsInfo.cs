using System.Collections;
using Avalonia.Controls.Utils;
using Avalonia.Styling;

namespace Avalonia.Controls.Presenters
{
    class VirtualizedRealizedItemsInfo
    {
        public int NumInFullView { get; private set; }

        internal double _currentOffset;
        public bool Vert { get;}
        public int Next => _firstInCache + _numInView;

        private double _panelOffset;
        private double _hiOffset;
        private double _hiCacheOffset;
        private ItemVirtualizingCache _cache;
        private int _firstInView;
        private int _lastInView => _firstInView + _numInView - 1;
        private int _lastInCache => _firstInCache + _numInCache - 1;
        private int _numInView;
        private int _numInCache;
        private int _firstInCache;
        private int _lastInFullView => _firstInView + NumInFullView - 1;
        private ITemplatedControl _templatedParent;
        private IEnumerable _items;
        private double _tempViewport;
        private double _tempAverageItem;
        public VirtualizedRealizedItemsInfo(bool vert, ItemVirtualizingCache cache, ITemplatedControl templatedParent)
        {
            Vert = vert;
            _cache = cache;
            _templatedParent = templatedParent;
        }
        public void SetFirst(IEnumerable items, int first)
        {
            _firstInView = first<0?0:first;
            _currentOffset = VirtualizingAverages.GetOffsetForIndex(_templatedParent, _firstInView, items, Vert);
            _numInView = 0;
            _numInCache = 0;
            NumInFullView = 0;
            _items = items;
            var av = VirtualizingAverages.GetEstimatedAverage(_templatedParent, _items, Vert);
            _tempAverageItem = Vert ? av.Height : av.Width;
        }

        internal void SetFirst( IEnumerable items)
        {
            _firstInCache = VirtualizingAverages.GetStartIndex(_templatedParent, _panelOffset-_cache.GetBackCacheSize(_tempViewport, _tempAverageItem), items, Vert);
            _firstInView = VirtualizingAverages.GetStartIndex(_templatedParent, _panelOffset, items, Vert);
            _currentOffset = VirtualizingAverages.GetOffsetForIndex(_templatedParent, _firstInCache, items, Vert);
            _numInView = 0;
            _numInCache = 0;
            NumInFullView = 0;
            _items = items;
             var av= VirtualizingAverages.GetEstimatedAverage(_templatedParent, _items, Vert);
            _tempAverageItem = Vert ? av.Height : av.Width;
        }

        internal void AddOffset(double offset)
        {
            var oldCurrentOffset = _currentOffset;
            _currentOffset += offset;
            if ((_currentOffset <= _hiOffset) && (_currentOffset > _panelOffset))
                NumInFullView++;
            if ((oldCurrentOffset <= _hiOffset) && (_currentOffset > _panelOffset))
                _numInView++;
            _numInCache++;
        }

        internal void SetPanelRelative(Point relPos, Size viewportSize)
        {
            _panelOffset = Vert ? relPos.Y : relPos.X;
            _currentOffset = _panelOffset;
            _hiOffset = _panelOffset + (Vert ? viewportSize.Height : viewportSize.Width);
            _tempViewport = Vert ? viewportSize.Height : viewportSize.Width;
            _hiCacheOffset = _hiOffset+_cache.GetFwdCacheSize(_tempViewport, _tempAverageItem);
        }

        internal bool RealizeNeeded(int numItems)
        {
            return _currentOffset < _hiCacheOffset && Next < numItems;
        }

        internal bool CheckForRemoval(int indx)
        {
            if ((indx < _firstInCache) || (indx > _lastInCache))
                return true;
            return false;
        }

        public override string ToString()
        {
            return $"{_panelOffset}:{_hiOffset}  {_firstInView}:{_lastInView}";
        }
    }
}

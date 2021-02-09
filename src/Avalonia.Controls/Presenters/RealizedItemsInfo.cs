using System.Collections;
using Avalonia.Collections;
using Avalonia.Controls.Utils;
using Avalonia.Styling;

namespace Avalonia.Controls.Presenters
{
    class RealizedItemsInfo
    {
        public int NumInFullView { get; protected set; }
        public int ScrollOffset { get; protected set; }

        internal double _currentOffset;
        public bool Vert { get;}
        public int Next => FirstInCache + _numInCache;

        private double _panelOffset;
        private double _hiOffset;
        private double _hiCacheOffset;
        protected ItemVirtualizingCache _cache;
        protected int _firstInView;
        private int _lastInView => _firstInView + _numInView - 1;
        private int _lastInCache => FirstInCache + _numInCache - 1;
        protected int _numInView;
        protected int _numInCache;
        public int FirstInCache { get; set; }
        protected ITemplatedControl _templatedParent;
        protected IEnumerable _items;
        protected double _tempViewport;
        protected double _tempAverageItem;
        public RealizedItemsInfo(IEnumerable items, bool vert, ItemVirtualizingCache cache, ITemplatedControl templatedParent)
        {
            Vert = vert;
            _cache = cache;
            _templatedParent = templatedParent;
            _items = items;
        }
        internal virtual void SetFirst(Vector scrollPos)
        {
            FirstInCache = VirtualizingAverages.GetStartIndex(_templatedParent, _panelOffset-_cache.GetBackCacheSize(_tempViewport, _tempAverageItem), _items, Vert);
            _firstInView = VirtualizingAverages.GetStartIndex(_templatedParent, _panelOffset, _items, Vert);
            _currentOffset = VirtualizingAverages.GetOffsetForIndex(_templatedParent, FirstInCache, _items, Vert);
            _numInView = 0;
            _numInCache = 0;
            NumInFullView = 0;
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
            if ((indx < FirstInCache) || (indx > _lastInCache))
                return true;
            return false;
        }

        public override string ToString()
        {
            return $"{_panelOffset}:{_hiOffset}  {_firstInView}:{_lastInView} {_items}";
        }
    }

    class RealizedItemsInfo2: RealizedItemsInfo
    {

        public RealizedItemsInfo2(IEnumerable items, bool vert, ItemVirtualizingCache cache, ITemplatedControl templatedParent): base(items,vert,cache,templatedParent)
        {
        }

        internal override void SetFirst(Vector scrollPos)
        {
            _firstInView = (int)(Vert ? scrollPos.Y : scrollPos.X);
            if (_items is IGroupingView gv)
                _firstInView = gv.GetLocalItemPosition(_firstInView);
            ScrollOffset = (int)(Vert ? scrollPos.Y : scrollPos.X)-_firstInView;
            _currentOffset = VirtualizingAverages.GetOffsetForIndex(_templatedParent, _firstInView, _items, Vert);
            FirstInCache = VirtualizingAverages.GetStartIndex(_templatedParent, _currentOffset - _cache.GetBackCacheSize(_tempViewport, _tempAverageItem), _items, Vert);
            _currentOffset = VirtualizingAverages.GetOffsetForIndex(_templatedParent, FirstInCache, _items, Vert);
            _numInView = 0;
            _numInCache = 0;
            NumInFullView = 0;
            var av = VirtualizingAverages.GetEstimatedAverage(_templatedParent, _items, Vert);
            _tempAverageItem = Vert ? av.Height : av.Width;
        }


    }
}

﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Avalonia.Collections
{
    public class GroupingViewInternal : IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Properties
        public object Name { get; set; }
        public bool IsGrouping => _groupPaths.Count > _groupLevel;
        public Dictionary<object, GroupingViewInternal> _groupIds = new Dictionary<object, GroupingViewInternal>();
        public int Count => _items.Count;

        #endregion

        #region Private Data
        private List<GroupDescription> _groupPaths;
        private int _groupLevel = 0;
        private event NotifyCollectionChangedEventHandler _collectionChanged;
        private AvaloniaList<object> _items { get; set; } = new AvaloniaList<object>();

        #endregion

        #region Events
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => _collectionChanged += value;
            remove => _collectionChanged -= value;
        }
        /// <summary>
        /// Raised when a property on the collection changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Constructor(s)
        public GroupingViewInternal(List<GroupDescription> groupPaths,object groupValue, int level)
        {
            _items = new AvaloniaList<object>();
            _groupPaths = groupPaths;
            _groupLevel = level;
            Name = groupValue;
        }

        #endregion

        #region Collection Methods

        internal void Add(object item)
        {
            if (IsGrouping)
            {
                var groupListItem = GetGroupForAdd(item, out var newlyCreated, out var indx);
                groupListItem.Add(item);
                if (newlyCreated)
                    NotifyAdd(groupListItem, indx);
            }
            else
            {
                var indx = ((IList)_items).Add(item);
                NotifyAdd(item, indx);
            }
        }

        internal void AddRange(IList items)
        {
            var tempList = new List<object>();
            foreach (var item in items)
                tempList.Add(item);
            AddRangeImpl(tempList);
        }
        private void AddRangeImpl(IEnumerable<object> items)
        {
            var indx = Count;
            if (IsGrouping)
            {
                Dictionary<GroupingViewInternal, List<object>> hiearchyList = new Dictionary<GroupingViewInternal, List<object>>();
                var newlyCreatedGroups = new List<GroupingViewInternal>();
                foreach (var item in items)
                {
                    var groupListItem = GetGroupForAdd(item, out var newlyCreated, out _);
                    if (!hiearchyList.TryGetValue(groupListItem, out var groupItems))
                    {
                        groupItems = new List<object>();
                        hiearchyList.Add(groupListItem, groupItems);
                    }
                    groupItems.Add(item);
                    if (newlyCreated)
                        newlyCreatedGroups.Add(groupListItem);
                }
                foreach (var group in hiearchyList)
                    group.Key.AddRangeImpl(group.Value);
                NotifyAddRange(newlyCreatedGroups, indx);
            }
            else
            {
                _items.AddRange(items);
                NotifyAddRange((IList)items, indx);
            }
        }

        internal bool Remove(object item)
        {
            if (IsGrouping)
            {
                if (GetGroupForRemove(item, out var groupListItem))
                {
                    var rem = groupListItem.Remove(item);
                    if (groupListItem.Count == 0)
                        RemoveGroupAndNotify(groupListItem);
                    return rem;
                }
                return false;
            }
            else
                return RemoveAndNotify(item);
        }
        internal void RemoveRange(IList items)
        {
            var tempList = new List<object>();
            foreach (var item in items)
                tempList.Add(item);
            RemoveRangeImpl(tempList);
        }
        private void RemoveRangeImpl(IEnumerable<object> items)
        {
            if (IsGrouping)
            {
                Dictionary<GroupingViewInternal, List<object>> hiearchyList = new Dictionary<GroupingViewInternal, List<object>>();
                var deletedGroups = new List<GroupingViewInternal>();
                foreach (var item in items)
                {
                    if (GetGroupForRemove(item, out var groupListItem))
                    {
                        if (!hiearchyList.TryGetValue(groupListItem, out var groupItems))
                        {
                            groupItems = new List<object>();
                            hiearchyList.Add(groupListItem, groupItems);
                        }
                        groupItems.Add(item);
                    }
                }
                foreach (var group in hiearchyList)
                {
                    group.Key.RemoveRangeImpl(group.Value);
                    if (group.Key.Count==0)
                        RemoveGroupAndNotify(group.Key);
                }
            }
            else
                RemoveRangeAndNotify(items);
        }

        private int IndexOf(object item)
        {
            return ((IList<object>)_items).IndexOf(item);
        }

        private void RemoveAt(int index)
        {
            ((IList<object>)_items).RemoveAt(index);
        }

        internal void Clear()
        {
            if (IsGrouping)
            {
                foreach (var group in _items)
                {
                    ((GroupingViewInternal)group).Clear();
                    _groupIds.Remove(((GroupingViewInternal)group).Name);
                }
            }
            _items.Clear();
            NotifyReset();
        }
        internal void ClearFrom(int level)
        {
            if (IsGrouping)
            {
                foreach (var group in _items)
                {
                    ((GroupingViewInternal)group).ClearFrom(level);
                    if (level <= _groupLevel)
                        _groupIds.Remove(((GroupingViewInternal)group).Name);
                }
            }
            if (level <= _groupLevel)
            {
                _items.Clear();
                NotifyReset();
            }
        }

        #endregion

        #region IAvaloniaList<object> Enumerators
        public IEnumerator<object> GetEnumerator()
        {
            return ((IEnumerable<object>)_items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_items).GetEnumerator();
        }

        #endregion

        #region Private Methods
        private void RemoveGroupAndNotify(GroupingViewInternal groupListItem)
        {
            _groupIds.Remove(groupListItem.Name);
            RemoveAndNotify(groupListItem);
        }
        private void RemoveRangeAndNotify(IEnumerable items)
        {
            foreach (var item in items)
                RemoveAndNotify(item);
        }
        private bool RemoveAndNotify(object value)
        {
            int index = IndexOf(value);
            if (index != -1)
            {
                RemoveAt(index);
                NotifyRemove(value, index);
                return true;
            }
            return false;
        }
        private object GetGroupValue(object item)
        {
            PropertyInfo info = item.GetType().GetProperty(_groupPaths[_groupLevel].GroupPath);
            var groupValue = info?.GetValue(item);
            if (groupValue == null)
                groupValue = _groupPaths[_groupLevel].NullStr;
            return groupValue;
        }
        private GroupingViewInternal GetGroupForAdd(object item, out bool newlyCreated, out int indx)
        {
            var groupValue = GetGroupValue(item);
            newlyCreated = false;
            indx = -1;
            if (!_groupIds.TryGetValue(groupValue, out var groupListItem))
            {
                groupListItem = new GroupingViewInternal(_groupPaths, groupValue, _groupLevel + 1);
                indx = ((IList)_items).Add(groupListItem);
                _groupIds.Add(groupValue, groupListItem);
                newlyCreated = true;
            }
            return groupListItem;
        }
        private bool GetGroupForRemove(object item, out GroupingViewInternal group)
        {
            var groupValue = GetGroupValue(item);
            return _groupIds.TryGetValue(groupValue, out group);
        }

        private static int _notifyCount;
        private void NotifyReset()
        {
            if (_collectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                _collectionChanged(this, e);
            }
            RaisePropertyChanged(nameof(Count));
        }
        private void NotifyRemove(object item, int index)
        {
            if (_collectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new[] { item }, index);
                _collectionChanged(this, e);
            }
            RaisePropertyChanged(nameof(Count));
        }
        private void NotifyAdd(object item, int index)
        {
            System.Console.WriteLine($"NotifyAdd  {++_notifyCount}");
            if (_collectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new[] { item }, index);
                _collectionChanged(this, e);
            }
            RaisePropertyChanged(nameof(Count));
        }
        private void NotifyAddRange(IList items, int index)
        {
            System.Console.WriteLine($"NotifyAddRange  {++_notifyCount}");
            if (_collectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items, index);
                _collectionChanged(this, e);
            }
            RaisePropertyChanged(nameof(Count));
        }

        #endregion

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
             => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public override string ToString()
        {
            return $"{Name}";
        }

    }

}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Avalonia.Collections
{
    public class GroupingViewInternal : IAvaloniaList<object>, IList, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Properties
        public object Name { get; set; }
        public bool IsGrouping => _groupPaths.Count > _groupLevel;
        public Dictionary<object, GroupingViewInternal> _groupIds = new Dictionary<object, GroupingViewInternal>();

        #region IAvaloniaList<object> Properties
        public int Count => ((IAvaloniaList<object>)_items).Count;

        public bool IsReadOnly => ((ICollection<object>)_items).IsReadOnly;

        object IReadOnlyList<object>.this[int index] => ((IReadOnlyList<object>)_items)[index];

        public object this[int index] { get => ((IAvaloniaList<object>)_items)[index]; set => ((IAvaloniaList<object>)_items)[index] = value; }
        #endregion
        #endregion

        #region Private Data
        private List<GroupPathInfo> _groupPaths;
        private int _groupLevel = 0;
        private event NotifyCollectionChangedEventHandler _collectionChanged;
        private AvaloniaList<object> _items { get; set; } = new AvaloniaList<object>();

        bool IList.IsFixedSize => throw new NotImplementedException();

        bool IList.IsReadOnly => throw new NotImplementedException();

        int ICollection.Count => Count;

        bool ICollection.IsSynchronized => throw new NotImplementedException();

        object ICollection.SyncRoot => throw new NotImplementedException();

        object IList.this[int index] { get => this[index]; set => this[index]=value; }

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
        public GroupingViewInternal(List<GroupPathInfo> groupPaths,object groupValue, int level)
        {
            _items = new AvaloniaList<object>();
            _groupPaths = groupPaths;
            _groupLevel = level;
            Name = groupValue;
        }

        #endregion

        #region IAvaloniaList<object> Methods
        public void AddRange(IEnumerable<object> items)
        {
            var indx = Count;
            if (IsGrouping)
            {
                Dictionary<GroupingViewInternal, List<object>> hiearchyList = new Dictionary<GroupingViewInternal, List<object>>();
                var newlyCreatedGroups = new List<GroupingViewInternal>();
                foreach (var item in items)
                {
                    var groupListItem = GetGroup(item, out var newlyCreated, out _);
                    if (!hiearchyList.TryGetValue(groupListItem,out var groupItems))
                    {
                        groupItems = new List<object>();
                        hiearchyList.Add(groupListItem, groupItems);
                    }
                    groupItems.Add(item);
                    if (newlyCreated)
                        newlyCreatedGroups.Add(groupListItem);
                }
                foreach (var group in hiearchyList)
                    group.Key.AddRange(group.Value);
                NotifyAddRange(newlyCreatedGroups, indx);
            }
            else
            {
                ((IAvaloniaList<object>)_items).AddRange(items);
                NotifyAddRange((IList)items,indx);
            }
        }

        public void InsertRange(int index, IEnumerable<object> items)
        {
            throw new NotSupportedException();
            ((IAvaloniaList<object>)_items).InsertRange(index, items);
        }

        public void Move(int oldIndex, int newIndex)
        {
            throw new NotSupportedException();
            ((IAvaloniaList<object>)_items).Move(oldIndex, newIndex);
        }

        public void MoveRange(int oldIndex, int count, int newIndex)
        {
            throw new NotSupportedException();
            ((IAvaloniaList<object>)_items).MoveRange(oldIndex, count, newIndex);
        }

        public void RemoveAll(IEnumerable<object> items)
        {
            ((IAvaloniaList<object>)_items).RemoveAll(items);
        }

        public void RemoveRange(int index, int count)
        {
            throw new NotSupportedException();
            ((IAvaloniaList<object>)_items).RemoveRange(index, count);
        }

        public int IndexOf(object item)
        {
            return ((IList<object>)_items).IndexOf(item);
        }

        public void Insert(int index, object item)
        {
            ((IList<object>)_items).Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ((IList<object>)_items).RemoveAt(index);
        }

        public void Add(object item)
        {
            if (IsGrouping)
            {
                var groupListItem = GetGroup(item, out var newlyCreated, out var indx);
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

        public void Clear()
        {
            if (IsGrouping)
            {
                foreach (var group in _items)
                    ((GroupingViewInternal)group).Clear();
            }
            ((ICollection<object>)_items).Clear();
            NotifyReset();
        }

        public bool Contains(object item)
        {
            return ((ICollection<object>)_items).Contains(item);
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            ((ICollection<object>)_items).CopyTo(array, arrayIndex);
        }

        public bool Remove(object item)
        {
            if (IsGrouping)
            {
                var groupListItem = GetGroup(item, out var _, out var _);
                var rem=groupListItem.Remove(item);
                if (groupListItem.Count == 0)
                    RemoveAndNotify(groupListItem);
                return rem;
            }
            else
                return RemoveAndNotify(item);
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
        private void RemoveAndNotify(int index)
        {
            if (index != -1)
            {
                var value = _items[index];
                ((IList)_items).RemoveAt(index);
                NotifyRemove(value, index);
            }
        }
        private object GetGroupValue(object item)
        {
            PropertyInfo info = item.GetType().GetProperty(_groupPaths[_groupLevel].GroupPath);
            var groupValue = info?.GetValue(item);
            if (groupValue == null)
                groupValue = _groupPaths[_groupLevel].NullStr;
            return groupValue;
        }
        private GroupingViewInternal GetGroup(object item, out bool newlyCreated, out int indx)
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

        private static int _notifyCount;
        private void NotifyReset()
        {
            if (_collectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                _collectionChanged(this, e);
            }
            RaisePropertyChanged("ItemCount");
        }
        private void NotifyRemove(object item, int index)
        {
            if (_collectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new[] { item }, index);
                _collectionChanged(this, e);
            }
            RaisePropertyChanged("ItemCount");
        }
        private void NotifyAdd(object item, int index)
        {
            System.Console.WriteLine($"NotifyAdd  {++_notifyCount}");
            if (_collectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new[] { item }, index);
                _collectionChanged(this, e);
            }
            RaisePropertyChanged("ItemCount");
        }
        private void NotifyAddRange(IList items, int index)
        {
            System.Console.WriteLine($"NotifyAddRange  {++_notifyCount}");
            if (_collectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items, index);
                _collectionChanged(this, e);
            }
            RaisePropertyChanged("ItemCount");
        }

        #endregion

        #region IList
        int IList.Add(object value)
        {
            int index = Count;
            Add(value);
            return index;
        }

        void IList.Clear()
        {
            throw new NotImplementedException();
        }

        bool IList.Contains(object value)
        {
            throw new NotImplementedException();
        }

        int IList.IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        void IList.Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        void IList.Remove(object value)
        {
            throw new NotImplementedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia.Diagnostics;

namespace Avalonia.Collections
{

    public class GroupViewList : IList , INotifyCollectionChanged
    {

        private List<GroupPathInfo> _groupPaths;
        public bool IsGrouping => Items.IsGrouping;
        public GroupViewListItem Items { get; set; }
        private AvaloniaList<object> _flatList;

        public bool IsFixedSize => ((IList)Items).IsFixedSize;

        public bool IsReadOnly => ((IList)Items).IsReadOnly;

        public int Count => ((ICollection)Items).Count;

        public bool IsSynchronized => ((ICollection)Items).IsSynchronized;

        public object SyncRoot => ((ICollection)Items).SyncRoot;

        public object this[int index] { get => ((IList)Items)[index]; set => ((IList)Items)[index] = value; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupList"/> class.
        /// </summary>
        public GroupViewList(AvaloniaList<object> items)
        {
            _flatList = items;
            _flatList.CollectionChanged += FlatCollectioChanged;
            _groupPaths = new List<GroupPathInfo>();
            _groupPaths.Add(new GroupPathInfo { GroupPath = "Group", NullStr = "No Group" });
            _groupPaths.Add(new GroupPathInfo { GroupPath = "Name", NullStr = "No Name" });
            Items = new GroupViewListItem(_groupPaths,"Root",0);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                ((INotifyCollectionChanged)Items).CollectionChanged += value;
            }

            remove
            {
                ((INotifyCollectionChanged)Items).CollectionChanged -= value;
            }
        }

        private void FlatCollectioChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                        AddToGroup(item);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                        RemoveFromGroup(item);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    break;
            }
        }

        public int Add(object value)
        {
            _flatList.Add(value);
            return AddToGroup(value);
        }

        public void Clear()
        {
            ((IList)Items).Clear();
        }

        public bool Contains(object value)
        {
            return ((IList)Items).Contains(value);
        }

        public int IndexOf(object value)
        {
            return ((IList)Items).IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            ((IList)Items).Insert(index, value);
        }

        public void Remove(object value)
        {
            _flatList.Remove(value);
            RemoveFromGroup(value);
        }

        public void RemoveAt(int index)
        {
            ((IList)Items).RemoveAt(index);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)Items).CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)Items).GetEnumerator();
        }

        private int AddToGroup(object value)
        {
            return ((IList)Items).Add(value);
        }
        private void RemoveFromGroup(object value)
        {
            ((IList)Items).Remove(value);
        }
    }

    public class GroupViewListItem : IList, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public object Name { get; set; }
        public int ItemCount => _items.Count;
        public bool IsGrouping => _groupPaths.Count() > _groupLevel;
        public Dictionary<object, GroupViewListItem> _groupIds = new Dictionary<object, GroupViewListItem>();
        private List<GroupPathInfo> _groupPaths;
        private int _groupLevel = 0;

        private event NotifyCollectionChangedEventHandler _collectionChanged;

        private IList _items { get; set; } = new AvaloniaList<object>();
        //public GroupViewListItem(object name, string groupPath)
        //{
        //    Name = name;
        //    _groupPath = groupPath;
        //}
        public GroupViewListItem(List<GroupPathInfo> groupPaths,object groupValue, int level)
        {
            _items = new List<object>();
            _groupPaths = groupPaths;
            _groupLevel = level;
            Name = groupValue;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => _collectionChanged += value;
            remove => _collectionChanged -= value;
        }
        /// <summary>
        /// Raised when a property on the collection changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsFixedSize => ((IList)_items).IsFixedSize;

        public bool IsReadOnly => ((IList)_items).IsReadOnly;

        public int Count => ((ICollection)_items).Count;

        public bool IsSynchronized => ((ICollection)_items).IsSynchronized;

        public object SyncRoot => ((ICollection)_items).SyncRoot;

        public object this[int index] { get => ((IList)_items)[index]; set => ((IList)_items)[index] = value; }

        public int Add(object value)
        {
            if (IsGrouping)
            {
                var groupListItem = GetGroup(value, out var newlyCreated, out var indx);
                groupListItem.Add(value);
                if (newlyCreated)
                    NotifyAdd(groupListItem, indx);
                return indx;
            }
            else
            {
                var indx= _items.Add(value);
                NotifyAdd(value, indx);
                return indx;
            }
        }
        public override string ToString()
        {
            return $"{Name}";
        }

        public void Clear()
        {
            ((IList)_items).Clear();
        }

        public bool Contains(object value)
        {
            return ((IList)_items).Contains(value);
        }

        public int IndexOf(object value)
        {
            return ((IList)_items).IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            ((IList)_items).Insert(index, value);
        }

        public void Remove(object value)
        {
            if (IsGrouping)
            {
                var groupListItem = GetGroup(value,out var _, out var _);
                groupListItem.Remove(value);
                if (groupListItem.ItemCount == 0)
                    RemoveAndNotify(groupListItem);
            }
            else
                RemoveAndNotify(value);
        }

        public void RemoveAt(int index)
        {
            ((IList)_items).RemoveAt(index);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)_items).CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)_items).GetEnumerator();
        }

        private void RemoveAndNotify(object value)
        {
            int index = IndexOf(value);
            if (index != -1)
            {
                RemoveAt(index);
                NotifyRemove(value, index);
            }
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

        private GroupViewListItem GetGroup(object item, out bool newlyCreated, out int indx)
        {
            var groupValue = GetGroupValue(item);
            newlyCreated = false;
            indx = -1;
            if (!_groupIds.TryGetValue(groupValue, out var groupListItem))
            {
                groupListItem = new GroupViewListItem(_groupPaths, groupValue, _groupLevel + 1);
                indx=_items.Add(groupListItem);
                _groupIds.Add(groupValue, groupListItem);
                newlyCreated = true;
            }
            return groupListItem;
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
            if (_collectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new[] { item }, index);
                _collectionChanged(this, e);
            }
            RaisePropertyChanged("ItemCount");
        }
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
             => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }

    public class GroupPathInfo
    {
        public string GroupPath { get; set; }
        public string NullStr { get; set; }
    }

}

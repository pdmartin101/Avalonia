using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Avalonia.Diagnostics;

namespace Avalonia.Collections
{

    public class GroupViewList : IList
    {

        private IList _internal { get; set; }
        private string _groupPath = "Group";
        private static object _nullGroup = "Null";
        public Dictionary<object, GroupViewListItem> _groupIds = new Dictionary<object, GroupViewListItem>();
        public AvaloniaList<GroupViewListItem> Groups { get; set; } = new AvaloniaList<GroupViewListItem>();
        public bool IsGrouping { get; set; } = true;
        public bool IsFixedSize => ((IList)_internal).IsFixedSize;

        public bool IsReadOnly => ((IList)_internal).IsReadOnly;

        public int Count => ((ICollection)_internal).Count;

        public bool IsSynchronized => ((ICollection)_internal).IsSynchronized;

        public object SyncRoot => ((ICollection)_internal).SyncRoot;

        public object this[int index] { get => ((IList)_internal)[index]; set => ((IList)_internal)[index] = value; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupList"/> class.
        /// </summary>
        public GroupViewList(IList list)
        {
            _internal = list;
        }

        public int Add(object value)
        {
            if (_groupPath != null)
            {
                PropertyInfo info = value.GetType().GetProperty(_groupPath);
                var groupValue = info?.GetValue(value);
                if (groupValue == null)
                    groupValue = _nullGroup;
                if (!_groupIds.TryGetValue(groupValue, out var groupListItem))
                {
                    groupListItem = new GroupViewListItem(groupValue, "Name");
                    Groups.Add(groupListItem);
                    _groupIds.Add(groupValue, groupListItem);
                }
                groupListItem.Add(value);
            }
            return ((IList)_internal).Add(value);
        }

        public void Clear()
        {
            ((IList)_internal).Clear();
        }

        public bool Contains(object value)
        {
            return ((IList)_internal).Contains(value);
        }

        public int IndexOf(object value)
        {
            return ((IList)_internal).IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            ((IList)_internal).Insert(index, value);
        }

        public void Remove(object value)
        {
            ((IList)_internal).Remove(value);
        }

        public void RemoveAt(int index)
        {
            ((IList)_internal).RemoveAt(index);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)_internal).CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)_internal).GetEnumerator();
        }
    }

    public class GroupViewListItem : IList
    {
        public object Name { get; set; }
        public int ItemCount => _items.Count;
        public bool IsGrouping => _groupPath != null;
        public Dictionary<object, GroupViewListItem> _groupIds = new Dictionary<object, GroupViewListItem>();
        private static object _nullGroup = "Null";
        private string _groupPath;
        public GroupViewListItem(object name, string groupPath)
        {
            Name = name;
            _groupPath = groupPath;
        }
        public AvaloniaList<object> _items { get; set; } = new AvaloniaList<object>();

        public bool IsFixedSize => ((IList)_items).IsFixedSize;

        public bool IsReadOnly => ((IList)_items).IsReadOnly;

        public int Count => ((ICollection)_items).Count;

        public bool IsSynchronized => ((ICollection)_items).IsSynchronized;

        public object SyncRoot => ((ICollection)_items).SyncRoot;

        public object this[int index] { get => ((IList)_items)[index]; set => ((IList)_items)[index] = value; }

        public void Add(object item)
        {
            if (IsGrouping)
            {
                PropertyInfo info = item.GetType().GetProperty(_groupPath);
                var groupValue = info?.GetValue(item);
                if (groupValue == null)
                    groupValue = _nullGroup;
                if (!_groupIds.TryGetValue(groupValue, out var groupListItem))
                {
                    groupListItem = new GroupViewListItem(groupValue, null);
                    _items.Add(groupListItem);
                    _groupIds.Add(groupValue, groupListItem);
                }
                groupListItem.Add(item);

            }
            else
                _items.Add(item);
        }
        public override string ToString()
        {
            return $"{Name}";
        }

        int IList.Add(object value)
        {
            return ((IList)_items).Add(value);
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
            ((IList)_items).Remove(value);
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
    }
}

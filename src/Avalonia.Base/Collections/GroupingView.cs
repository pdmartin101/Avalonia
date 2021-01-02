using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia.Diagnostics;

namespace Avalonia.Collections
{

    public class GroupingView : IAvaloniaList<object>, IList , INotifyCollectionChanged
    {

        #region Properties
        public bool IsGrouping => Items.IsGrouping;
        public GroupingViewInternal Items { get; set; }

        #region IAvaloniaList<object> Properties
        public int Count => ((IAvaloniaList<object>)Items).Count;

        public bool IsReadOnly => ((ICollection<object>)Items).IsReadOnly;

        object IReadOnlyList<object>.this[int index] => ((IReadOnlyList<object>)Items)[index];

        public object this[int index] { get => ((IAvaloniaList<object>)Items)[index]; set => ((IAvaloniaList<object>)Items)[index] = value; }

        #endregion

        #region IList Properties
        bool IList.IsFixedSize => throw new NotImplementedException();

        bool ICollection.IsSynchronized => throw new NotImplementedException();

        object ICollection.SyncRoot => throw new NotImplementedException();

        bool IList.IsReadOnly => throw new NotImplementedException();

        int ICollection.Count => Count;

        object IList.this[int index] { get => this[index]; set => this[index]=value; }


        #endregion

        #endregion

        #region Events
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { ((INotifyCollectionChanged)Items).CollectionChanged += value; }
            remove { ((INotifyCollectionChanged)Items).CollectionChanged -= value; }
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add { ((INotifyPropertyChanged)Items).PropertyChanged += value; }
            remove { ((INotifyPropertyChanged)Items).PropertyChanged -= value; }
        }
        #endregion

        #region Private Data
        private List<GroupPathInfo> _groupPaths;
        private AvaloniaList<object> _flatList;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupingView"/> class.
        /// </summary>

        #region Constructor(s)
        public GroupingView(AvaloniaList<object> items)
        {
            _flatList = items;
            _flatList.CollectionChanged += FlatCollectioChanged;
            _groupPaths = new List<GroupPathInfo>();
            _groupPaths.Add(new GroupPathInfo { GroupPath = "Group", NullStr = "No Group" });
            _groupPaths.Add(new GroupPathInfo { GroupPath = "Name", NullStr = "No Name" });
            Items = new GroupingViewInternal(_groupPaths,"Root",0);
        }

        #endregion

        #region Public Methods
        public void AddGroup(GroupPathInfo groupPath)
        {
            _groupPaths.Add(groupPath);
        }

        #region IAvaloniaList<object> Methods
        public void AddRange(IEnumerable<object> items)
        {
            ((IAvaloniaList<object>)Items).AddRange(items);
        }

        public void InsertRange(int index, IEnumerable<object> items)
        {
            ((IAvaloniaList<object>)Items).InsertRange(index, items);
        }

        public void Move(int oldIndex, int newIndex)
        {
            ((IAvaloniaList<object>)Items).Move(oldIndex, newIndex);
        }

        public void MoveRange(int oldIndex, int count, int newIndex)
        {
            ((IAvaloniaList<object>)Items).MoveRange(oldIndex, count, newIndex);
        }

        public void RemoveAll(IEnumerable<object> items)
        {
            ((IAvaloniaList<object>)Items).RemoveAll(items);
        }

        public void RemoveRange(int index, int count)
        {
            ((IAvaloniaList<object>)Items).RemoveRange(index, count);
        }

        public int IndexOf(object item)
        {
            return ((IList<object>)Items).IndexOf(item);
        }

        public void Insert(int index, object item)
        {
            ((IList<object>)Items).Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ((IList<object>)Items).RemoveAt(index);
        }

        public void Add(object item)
        {
            _flatList.Add(item);
            AddToHierarchy(item);
        }

        public void Clear()
        {
            ((ICollection<object>)Items).Clear();
        }

        public bool Contains(object item)
        {
            return ((ICollection<object>)Items).Contains(item);
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            ((ICollection<object>)Items).CopyTo(array, arrayIndex);
        }

        public bool Remove(object item)
        {
            _flatList.Remove(item);
            return RemoveFromHierarchy(item);
        }

        #endregion

        #endregion

        #region IAvaloniaList<object> Enumerators
        public IEnumerator<object> GetEnumerator()
        {
            return ((IEnumerable<object>)Items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Items).GetEnumerator();
        }
        #endregion

        #region Private Methods
        private int AddToHierarchy(object value)
        {
            return ((IList)Items).Add(value);
        }
        private bool RemoveFromHierarchy(object value)
        {
            return Items.Remove(value);
        }
        private void FlatCollectioChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                        AddToHierarchy(item);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                        RemoveFromHierarchy(item);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Clear();
                    break;
                default:
                    break;
            }
        }

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
        #endregion

    }

    public class GroupPathInfo
    {
        public string GroupPath { get; set; }
        public string NullStr { get; set; }
    }

}

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Avalonia.Collections
{

    public class GroupingView :IEnumerable,  INotifyCollectionChanged
    {

        #region Properties
        public bool IsGrouping => Items.IsGrouping;
        public GroupingViewInternal Items { get; set; }

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

        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupingView"/> class.
        /// </summary>

        public GroupingView(AvaloniaList<object> items)
        {
            _flatList = items;
            _flatList.CollectionChanged += FlatCollectioChanged;
            _groupPaths = new List<GroupPathInfo>();
            _groupPaths.Add(new GroupPathInfo { GroupPath = "Group", NullStr = "No Group" });
            //_groupPaths.Add(new GroupPathInfo { GroupPath = "Name", NullStr = "No Name" });
            Items = new GroupingViewInternal(_groupPaths,"Root",0);
        }

        #endregion

        #region Public Methods
        public void AddGroup(GroupPathInfo groupPath)
        {
            Items.ClearFrom(_groupPaths.Count - 1);   // -1 due to ItemsGenerator changing to GroupGenerator in the ItemsPresenter
            _groupPaths.Add(groupPath);
            Items.AddRange(_flatList);
        }

        public void RemoveGroup(int indx)
        {
            if (indx > 0 && indx < _groupPaths.Count)
            {
                var clearFrom = indx;
                if (clearFrom == (_groupPaths.Count - 1))
                    clearFrom--;  // -1 due to GroupGenerator changing to ItemsGenerator in the ItemsPresenter
                Items.ClearFrom(clearFrom);
                _groupPaths.RemoveAt(indx);
                Items.AddRange(_flatList);
            }
        }

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
        private void FlatCollectioChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Items.AddRange(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Items.RemoveRange(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Items.Clear();
                    break;
                default:
                    break;
            }
        }
        #endregion

    }

    public class GroupPathInfo
    {
        public string GroupPath { get; set; }
        public string NullStr { get; set; }
    }

}

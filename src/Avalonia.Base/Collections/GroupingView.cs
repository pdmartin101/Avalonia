using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Avalonia.Collections
{
    public interface IGroupingView : IEnumerable
    {
        public object Name { get; }
        public bool IsGrouping { get; }

        public IGroupingView Items { get; }
        int Count { get; }
        int TotalItems { get; }
        int TotalGroups { get; }
        int ItemScrollStart { get; }
        int ItemScrollEnd { get; }
        int GetLocalItemPosition(int scrollVal);
    }
    public class GroupingView : AvaloniaObject, IGroupingView, INotifyCollectionChanged
    {
        public static readonly DirectProperty<GroupingView, int> TestProperty =
            AvaloniaProperty.RegisterDirect<GroupingView, int>(nameof(Test), o => o.Test, (o, v) => o.Test = v);
        public static readonly DirectProperty<GroupingView, AvaloniaList<object>> SourceProperty =
            AvaloniaProperty.RegisterDirect<GroupingView, AvaloniaList<object>>(nameof(Source), o => o.Source, (o, v) => o.Source = v);

        public static Dictionary<string, GroupingView> GroupingViews = new Dictionary<string, GroupingView>();
        #region Properties
        //        public bool IsGrouping => Items.IsGrouping;
        public string Id { get => _id; set => SetId(value); }

        private void SetId(string value)
        {
            _id = value;
            GroupingViews.Add(_id, this);
        }

        public AvaloniaList<object> Source
        {
            get { return _source; }
            set { SetSource(value); }
        }
        public List<GroupDescription> GroupDescriptions
        {
            get { return _groupDescriptions; }
            set { SetGroupDescriptions(value); }
        }
        public int Test
        {
            get { return _test; }
            set { SetTest(value); }
        }
        #endregion

        #region IGroupingView

        object IGroupingView.Name => ((IGroupingView)_internalItems).Name;
        IGroupingView IGroupingView.Items => _internalItems;
        int IGroupingView.Count => _internalItems.Count;

        bool IGroupingView.IsGrouping => ((IGroupingView)_internalItems).IsGrouping;
        int IGroupingView.TotalItems => ((IGroupingView)_internalItems).TotalItems;
        int IGroupingView.TotalGroups => ((IGroupingView)_internalItems).TotalGroups;
        int IGroupingView.ItemScrollStart => ((IGroupingView)_internalItems).ItemScrollStart;
        int IGroupingView.ItemScrollEnd => ((IGroupingView)_internalItems).ItemScrollEnd;
        int IGroupingView.GetLocalItemPosition(int scrollVal)
        {
            return ((IGroupingView)_internalItems).GetLocalItemPosition(scrollVal);
        }

        #endregion

        #region Events
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { ((INotifyCollectionChanged)_internalItems).CollectionChanged += value; }
            remove { ((INotifyCollectionChanged)_internalItems).CollectionChanged -= value; }
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add { ((INotifyPropertyChanged)_internalItems).PropertyChanged += value; }
            remove { ((INotifyPropertyChanged)_internalItems).PropertyChanged -= value; }
        }
        #endregion

        #region Private Data
        private GroupingViewInternal _internalItems;
        private List<GroupDescription> _groupDescriptions;
        private AvaloniaList<object> _source = new AvaloniaList<object>();
        private int _test = 77;
        private string _id;
        #endregion

        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupingView"/> class.
        /// </summary>

        //public GroupingView(AvaloniaList<object> items)
        //{
        //    Source = items;
        //    Source.CollectionChanged += FlatCollectioChanged;
        //    _groupPaths = new List<GroupPathInfo>();
        //    _groupPaths.Add(new GroupPathInfo { GroupPath = "Group", NullStr = "No Group" });
        //    //_groupPaths.Add(new GroupPathInfo { GroupPath = "Name", NullStr = "No Name" });
        //    Items = new GroupingViewInternal(_groupPaths, "Root", 0);
        //}
        public GroupingView()
        {
            _groupDescriptions = new List<GroupDescription>();
            _internalItems = new GroupingViewInternal(_groupDescriptions, "Root", 0);
            _internalItems.SetItemScrolling(-1);
        }

        #endregion

        #region Public Methods
        public void AddGroup(GroupDescription groupPath)
        {
            _internalItems.ClearFrom(_groupDescriptions.Count - 1);   // -1 due to ItemsGenerator changing to GroupGenerator in the ItemsPresenter
            _groupDescriptions.Add(groupPath);
            _internalItems.AddRange(Source);
        }

        public void RemoveGroup(int indx)
        {
            if (indx > 0 && indx < _groupDescriptions.Count)
            {
                var clearFrom = indx;
                if (clearFrom == (_groupDescriptions.Count - 1))
                    clearFrom--;  // -1 due to GroupGenerator changing to ItemsGenerator in the ItemsPresenter
                _internalItems.ClearFrom(clearFrom);
                _groupDescriptions.RemoveAt(indx);
                _internalItems.AddRange(Source);
            }
        }

        //public object GetItemFromScrollpos(int scrollPos)
        //{
        //    return ((GroupingViewInternal)_internalItems).GetItemFromScrollpos(scrollPos);
        //}
        #endregion

        #region IAvaloniaList<object> Enumerators
        //public IEnumerator<object> GetEnumerator()
        //{
        //    return ((IEnumerable<object>)Items).GetEnumerator();
        //}

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_internalItems).GetEnumerator();
        }
        #endregion

        #region Private Methods

        void ReSortAll()
        {

        }
        private void SetSource(AvaloniaList<object> value)
        {
            _source = value;
            if (value != null)
            {
                _source.CollectionChanged += FlatCollectioChanged;
                _internalItems.AddRange(value);
                _internalItems.SetItemScrolling(-1);
            }
        }
        private void SetGroupDescriptions(List<GroupDescription> value)
        {
            _groupDescriptions = value;
        }
        private void SetTest(int value)
        {
            _test = value;
        }
        private void FlatCollectioChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    _internalItems.AddRange(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    _internalItems.RemoveRange(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _internalItems.Clear();
                    break;
                default:
                    break;
            }
        }
        #endregion

        public override string ToString()
        {
            return $"{((IGroupingView)_internalItems).Name}:{_internalItems.Count} {((IGroupingView)_internalItems).IsGrouping}";
        }

    }

    public class GroupDescription
    {
        public string GroupPath { get; set; }
        public string NullStr { get; set; }
    }

    public class GroupingItemScrolling
    {
        public object Item { get; set; }
//        public List<int> GroupPositions { get; set; } = new List<int>();
    }

}

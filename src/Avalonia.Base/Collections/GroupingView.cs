using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Avalonia.Collections
{

    public class GroupingView : AvaloniaObject, IEnumerable, INotifyCollectionChanged
    {
        public static readonly DirectProperty<GroupingView, int> TestProperty =
            AvaloniaProperty.RegisterDirect<GroupingView, int>(nameof(Test), o => o.Test, (o, v) => o.Test = v);
        public static readonly DirectProperty<GroupingView, AvaloniaList<object>> SourceProperty =
            AvaloniaProperty.RegisterDirect<GroupingView, AvaloniaList<object>>(nameof(Source), o => o.Source, (o, v) => o.Source = v);

        public static Dictionary<string, GroupingView> GroupingViews = new Dictionary<string, GroupingView>();
        #region Properties
        public bool IsGrouping => Items.IsGrouping;
        public GroupingViewInternal Items { get; private set; }
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
            Items = new GroupingViewInternal(_groupDescriptions, "Root", 0);
        }

        #endregion

        #region Public Methods
        public void AddGroup(GroupDescription groupPath)
        {
            Items.ClearFrom(_groupDescriptions.Count - 1);   // -1 due to ItemsGenerator changing to GroupGenerator in the ItemsPresenter
            _groupDescriptions.Add(groupPath);
            Items.AddRange(Source);
        }

        public void RemoveGroup(int indx)
        {
            if (indx > 0 && indx < _groupDescriptions.Count)
            {
                var clearFrom = indx;
                if (clearFrom == (_groupDescriptions.Count - 1))
                    clearFrom--;  // -1 due to GroupGenerator changing to ItemsGenerator in the ItemsPresenter
                Items.ClearFrom(clearFrom);
                _groupDescriptions.RemoveAt(indx);
                Items.AddRange(Source);
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

        void ReSortAll()
        {

        }
        private void SetSource(AvaloniaList<object> value)
        {
            _source = value;
            if (value != null)
            {
                _source.CollectionChanged += FlatCollectioChanged;
                Items.AddRange(value);
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

    public class GroupDescription
    {
        public string GroupPath { get; set; }
        public string NullStr { get; set; }
    }

}

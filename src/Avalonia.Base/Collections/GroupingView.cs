using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Avalonia.Collections
{

    public class GroupingView : AvaloniaObject,IEnumerable,  INotifyCollectionChanged
    {
        public static readonly DirectProperty<GroupingView, int> TestProperty =
            AvaloniaProperty.RegisterDirect<GroupingView, int>(nameof(Test), o => o.Test, (o, v) => o.Test = v);
        public static readonly DirectProperty<GroupingView, AvaloniaList<object>> SourceProperty =
            AvaloniaProperty.RegisterDirect<GroupingView, AvaloniaList<object>>(nameof(Source), o => o.Source, (o, v) => o.Source = v);


        #region Properties
        public bool IsGrouping => Items.IsGrouping;
        public GroupingViewInternal Items { get; private set; }
        public AvaloniaList<object> Source
        {
            get { return _source; }
            set { SetSource(value); }
        }
        public List<GroupDescription> GroupPaths
        {
            get { return _groupPaths; }
            set { SetGroups(value); }
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
        private List<GroupDescription> _groupPaths;
        private AvaloniaList<object> _source = new AvaloniaList<object>();
        private int _test = 77;
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
            _groupPaths = new List<GroupDescription>();
            //_groupPaths.Add(new GroupPathInfo { GroupPath = "Group", NullStr = "No Group" });
            //_groupPaths.Add(new GroupPathInfo { GroupPath = "Name", NullStr = "No Name" });
            Items = new GroupingViewInternal(_groupPaths, "Root", 0);
        }

        #endregion

        #region Public Methods
        public void AddGroup(GroupDescription groupPath)
        {
            Items.ClearFrom(_groupPaths.Count - 1);   // -1 due to ItemsGenerator changing to GroupGenerator in the ItemsPresenter
            _groupPaths.Add(groupPath);
            Items.AddRange(Source);
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
        private void SetSource(AvaloniaList<object> value)
        {
            _source = value;
            if (value != null)
                Items.AddRange(value);
        }
        private void SetGroups(List<GroupDescription> value)
        {
            _groupPaths = value;
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia.Diagnostics;

namespace Avalonia.Collections
{
    public class GroupList : AvaloniaList<object>
    {

        public AvaloniaList<GroupListItem> Groups { get; set; } = new AvaloniaList<GroupListItem>();
        public Dictionary<string, GroupListItem> _groupIds = new Dictionary<string, GroupListItem>();
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupList"/> class.
        /// </summary>
        public GroupList()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupList"/> class.
        /// </summary>
        /// <param name="items">The initial items for the collection.</param>
        public GroupList(IEnumerable<object> items) : base(items)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupList"/> class.
        /// </summary>
        /// <param name="items">The initial items for the collection.</param>
        public GroupList(params object[] items) : base(items)
        {
        }

        public void Add(string groupId,object item)
        {
            if (!_groupIds.TryGetValue(groupId, out var group))
            {
                group = new GroupListItem(groupId);
                Groups.Add(group);
                _groupIds.Add(groupId, group);
            }
            group.Items.Add(item);
            base.Add(item);
        }

    }

    public class GroupListItem 
    {
        public string Name { get; set; }
        public GroupListItem(string name)
        {
            Name = name;
        }
        public AvaloniaList<object> Items { get; set; } = new AvaloniaList<object>();
        public override string ToString()
        {
            return Name;
        }
    }
}

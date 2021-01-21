using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Todo
{
    public class TodoDatabase : ScriptableObject
    {
        [SerializeField]
        private List<TodoGroup> _groups;
        public List<TodoGroup> groups { get { return _groups; }}      

        public TodoDatabase()
        {
            _groups = new List<TodoGroup>();
        }

        /**
         * ? return all groups if tag is null
         */
        public List<TodoGroup> GetVisibleGroups(Tag tag)
        {
            return tag != null ? _groups.FindAll(group => group.tag.index == tag.index) : _groups;
        }

        public void AddGroup(ref TodoGroup group)
        {
            _groups.Add(group);
        }

        public void RemoveGroup(ref TodoGroup group)
        {
            _groups.Remove(group);
        }


        /**
         * Sync tag reference of each group and config's tag
         */
        public void SyncTagReferences(ref TodoConfig config)
        {
            foreach (TodoGroup group in _groups)
                group.tag = config.GetTagByIndex(group.tag.index);
        }
    }
}
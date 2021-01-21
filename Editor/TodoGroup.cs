using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Todo
{
    [Serializable]
    public class TodoGroup
    {
        public string title;
        public string note;
        public Tag tag;
        public UnityEngine.Object reference;


        [SerializeField]
        private List<Todo> _todos;
        public List<Todo> todos { get { return _todos; }}        


        public TodoGroup()
        {
            title = "New group";
            note = "";
            _todos = new List<Todo>();
        }


        /**
         * ! Create a TodoGroup with this method.
         */
        public TodoGroup(string title, Tag tag) : this()
        {
            this.title = title;
            this.tag = tag;
        }

        public void AddTodo(ref Todo todo)
        {
            _todos.Add(todo);
        }

        public void RemoveTodo(ref Todo todo)
        {
            _todos.Remove(todo);
        }
    }

}
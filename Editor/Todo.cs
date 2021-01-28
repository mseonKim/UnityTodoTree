using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Todo
{
	[Serializable]
	public class Todo
	{

		public string title;
		public string description;

		public Progress progress;
		public Priority priority;

		public DateTime? createdDate { get; }
		public DateTime? endDate;

		[HideInInspector]
        public bool isVisible = true;

		public Todo()
		{
			title = "new todo";
			description = "";
		}


		/**
		 * ! Create a Todo with this method in the todo tree editor.
		 */
		public Todo(string title, string desc, Progress progress, Priority priority, DateTime? endDate)
		{
			this.title = title;
			description = desc;
			this.progress = progress;
			this.priority = priority;
			this.createdDate = DateTime.Now;
			this.endDate = endDate;
		}

	}
}

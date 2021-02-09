using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Todo
{
	[Serializable]
	public class Tag
	{
		public string name;
		public Color color;
		[HideInInspector]
		public int index;

		public Tag(string name, Color color, int index)
		{
			this.name = name;
			this.color = color;
			this.index = index;
		}
	}

	[Serializable]
	public class Priority
	{
		public string name;
		public Color color;
		[HideInInspector]
		public int index;

		public Priority(string name, Color color)
		{
			this.name = name;
			this.color = color;
		}
	}

	[Serializable]
	public class Progress
	{
		public string status;
		public Color color;
		[HideInInspector]
		public int index;

		public Progress(string status, Color color)
		{
			this.status = status;
			this.color = color;
		}
	}

	public class TodoConfig : ScriptableObject
	{
		public bool inpsectorGUI = true;
		[SerializeField]
		private List<Tag> _tags;
		public List<Tag> tags { get { return _tags; } }
		[SerializeField]
		private List<Priority> _priorities;
		[SerializeField]
		private List<Progress> _progresses;


		public TodoConfig()
		{
			SetDefaultTags();
			SetDefaultPriorities();
			SetDefaultProgresses();
		}

		private void SetDefaultTags()
		{
			_tags = new List<Tag>();
			_tags.Capacity = 2;

			_tags.Add(new Tag("TODO", new Color(1f, 0.9f, 0.19f), 0));
			_tags.Add(new Tag("FIX ME", new Color(1f, 0.2f, 0.23f), 1));
		}

		private void SetDefaultPriorities()
		{
			_priorities = new List<Priority>();
			_priorities.Capacity = 4;

			_priorities.Add(new Priority("Default", Color.white));
			_priorities.Add(new Priority("Minor", new Color(1f, 0.72f, 0f)));
			_priorities.Add(new Priority("Medium", new Color(0f, 1f, 0.32f)));
			_priorities.Add(new Priority("Major", new Color(1f, 0.2f, 0.2f)));
		}

		private void SetDefaultProgresses()
		{
			_progresses = new List<Progress>();
			_progresses.Capacity = 4;

			_progresses.Add(new Progress("Default", Color.white));
			_progresses.Add(new Progress("Active", new Color(0f, 1f, 0.67f)));
			_progresses.Add(new Progress("OnHold", new Color(0.4f, 0.5f, 1f)));
			_progresses.Add(new Progress("Completed", Color.yellow));
		}

		public int GetTagCount() { return _tags.Count; }
		public Tag GetTagByIndex(int index) { return index < _tags.Count ? _tags[index] : _tags[0]; }
		public Priority GetPriorityByIndex(int index) { return index < _priorities.Count ? _priorities[index] : _priorities[0]; }
		public Progress GetProgressByIndex(int index) { return index < _progresses.Count ? _progresses[index] : _progresses[0]; }

		public string[] GetTagNames()
		{
			string[] names = new string[_tags.Count];
			for (int i = 0; i < _tags.Count; i++)
				names[i] = _tags[i].name;

			return names;
		}

		public string[] GetPriorityNames()
		{
			string[] names = new string[_priorities.Count];
			for (int i = 0; i < _priorities.Count; i++)
				names[i] = _priorities[i].name;

			return names;
		}

		public string[] GetProgressStatuses()
		{
			string[] statuses = new string[_progresses.Count];
			for (int i = 0; i < _progresses.Count; i++)
				statuses[i] = _progresses[i].status;

			return statuses;
		}

		public void AddTag(ref Tag tag) { _tags.Add(tag); }
		public void RemoveTag(ref Tag tag)
		{
			if (_tags.Count < 2)
				return;

			_tags.Remove(tag);
		}

		public void AdjustConfigIndices()
		{
			// Tag
			for (int i = 0; i < _tags.Count; i++)
				_tags[i].index = i;

			// Priority
			for (int i = 0; i < _priorities.Count; i++)
				_priorities[i].index = i;

			// Progress
			for (int i = 0; i < _progresses.Count; i++)
				_progresses[i].index = i;
		}

	}
}

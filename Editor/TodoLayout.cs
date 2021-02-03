using System;
using UnityEngine;

namespace UnityEditor.Todo
{
    public enum TodoLayout
	{
		ToolbarHeight = 24,
		MinSearchbarWidth = 250,
		SidebarSpace = 4,
		MinSidebarWidth = 120,
		MaxSidebarWidth = 480,
		TodoGroupFieldHeight = 30,
		MinTodoGroupHeight = 200,
		TodoGroupTitleHeight = 25,
		TodoGroupSpace = 5,
        CreateGroupButtonHeight = 30,
		TagColorWidth = 50,
		TodoFieldHeight = 90,
		MinTodoTitleWidth = 100,
		MinPriorityPopupWidth = 80,
		TodoAddButtonHeight = 28
	}

	public class HorizontalGroup: IDisposable
	{
		public HorizontalGroup(params GUILayoutOption[] options)
		{
			GUILayout.BeginHorizontal(options);
		}

		public HorizontalGroup(GUIStyle style, params GUILayoutOption[] options)
		{
			GUILayout.BeginHorizontal(style, options);
		}

		public void Dispose()
		{
			GUILayout.EndHorizontal();	
		}
	}

	public class VerticalGroup: IDisposable
	{
		public VerticalGroup(params GUILayoutOption[] options)
		{
			GUILayout.BeginVertical(options);
		}

		public VerticalGroup(GUIStyle style, params GUILayoutOption[] options)
		{
			GUILayout.BeginVertical(style, options);
		}

		public void Dispose()
		{
			GUILayout.EndVertical();	
		}
	}

	public class ScrollViewGroup: IDisposable
	{
		public ScrollViewGroup(ref Vector2 pos, params GUILayoutOption[] options)
		{
			pos = GUILayout.BeginScrollView(pos, options);
		}

		public void Dispose()
		{
			GUILayout.EndScrollView();
		}
	}
}
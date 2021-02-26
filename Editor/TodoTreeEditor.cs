using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Todo
{
	public class TodoTreeEditor : EditorWindow
	{
		// Singleton
		private static TodoTreeEditor _editor;
		
		// Data
		private string _todoDirectoryPath = "/Editor/TodoTree/";
		private string _todoConfigPath = "Assets/Editor/TodoTree/TodoConfig.asset";
		private string _todoDatabasePath = "Assets/Editor/TodoTree/TodoDatabase.asset";
		private TodoConfig _config;
		private TodoDatabase _data;

		// Controllers
		private List<TodoGroup> _currentGroups;
		private Tag _currentTag;
		private TodoGroup _selectedGroup;
		private Todo _selectedTodo;
		private bool _showTodoGroup;
		private bool _hasSelectedTodoChanged;
		private string _searchString = "";
		private string _preSearchString = "";
		private Vector2 _sidebarScrollPos;
        private Vector2 _todoAreaScrollPos;
		private bool _hasTodoDragStarted;
		private float _startTodoDragPosY;
		private int _startTodoDragIndex = -1;			// -1: null, others: valid indices
		private int _expectedTodoDragIndex = -1;		// -1: null, others: valid indices
		

		[MenuItem("Window/TODO Tree")]
		internal static void InitTree()
		{
			_editor = EditorWindow.GetWindow<TodoTreeEditor>();
			_editor.minSize = new Vector2(400, 300);
			_editor.titleContent = new GUIContent("ToDo Tree");
		}
		
		void OnEnable()
		{
			Initialize();
		}

		void OnGUI()
		{
			// Return if data not loaded
			if (_config == null || _data == null)
			{
				GUILayout.Label("No data loaded", EditorStyles.centeredGreyMiniLabel);
				return;
			}

			// Display Todo Tree
			Toolbar();
			using (new HorizontalGroup())
			{
				if (_currentGroups != null)
				{
					Sidebar();
					TodoArea();
				}
			}
		}


		private void Initialize()
		{
			// Load config if no cached one.
			if (_config == null)
				LoadTodoConfig();

			// Load data if no cached one.
			if (_data == null)
				LoadTodoDatabase();

			// Set settings
			_config.AdjustConfigIndices();
			_data.SyncTagReferences(ref _config);
			SetCurrentTag(null);
		}

		private void LoadTodoConfig()
		{
			// Create a new one if config doesn't exist.
			_config = AssetDatabase.LoadAssetAtPath<TodoConfig>(_todoConfigPath) ?? CreateTodoConfig();
		}

		private void LoadTodoDatabase()
		{
			// Create a new one if config doesn't exist.
			_data = AssetDatabase.LoadAssetAtPath<TodoDatabase>(_todoDatabasePath) ?? CreateTodoDatabase();
		}

		private TodoConfig CreateTodoConfig()
		{
			// Create default config
			TodoConfig config = ScriptableObject.CreateInstance<TodoConfig>();
			System.IO.Directory.CreateDirectory(Application.dataPath + _todoDirectoryPath);
			AssetDatabase.CreateAsset(config, _todoConfigPath);
			GUI.changed = true;
			return config;
		}

		private TodoDatabase CreateTodoDatabase()
		{
			// Create default database
			TodoDatabase db = ScriptableObject.CreateInstance<TodoDatabase>();
			System.IO.Directory.CreateDirectory(Application.dataPath + _todoDirectoryPath);
			AssetDatabase.CreateAsset(db, _todoDatabasePath);
			GUI.changed = true;
			return db;
		}

		private void Toolbar()
		{
			GUIStyle style = new GUIStyle(EditorStyles.toolbar);
			style.fixedHeight = (float)TodoLayout.ToolbarHeight;
			// Draw Tags & Search bar
			using (new HorizontalGroup(style))
			{
				TagField();
				GUILayout.FlexibleSpace();
				DrawSearchbar();
			}
		}

		private void Sidebar()
		{
			// Draw Layout
			float sidebarWidth = Screen.width > 1440f ? (float)TodoLayout.MaxSidebarWidth : Mathf.Max((float)TodoLayout.MinSidebarWidth, Screen.width / 3f);
			using (new VerticalGroup(GUI.skin.box, GUILayout.Width(sidebarWidth), GUILayout.ExpandHeight(true)))
			{
				using (new ScrollViewGroup(ref _sidebarScrollPos))
				{
					// Fill contents
					for (int i = 0; i < _currentGroups.Count; i++)
					{
						if (_searchString.Length == 0 || _currentGroups[i].isVisible)
						{
							TodoGroupField(i);
							GUILayout.Space((float)TodoLayout.SidebarSpace);
						}
					}
				}

				// Hide group if empty space clicked
				Event e = Event.current;
				if (e.isMouse && e.type == EventType.MouseDown
					&& GUILayoutUtility.GetLastRect().Contains(e.mousePosition))
				{
					HideTodoGroup();
					return;
				}
				
				// Display selected Todo group
				DisplaySelectedTodoGroup();

				// Display Add & Remove group butons
				DisplayTodoGroupButtons();
			}
		}

		private void TodoArea()
		{
			Event e = Event.current;

			// Draw Layout
			using (new VerticalGroup(GUI.skin.box, GUILayout.ExpandWidth(true)))
			{
				using (new ScrollViewGroup(ref _todoAreaScrollPos))
				{
					if (_selectedGroup == null)
					{
						GUILayout.Label("No asset selected.");
						return;
					}

					// Show group's title
					GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
					labelStyle.fontSize = 26;
					labelStyle.fontStyle = FontStyle.Bold;
					GUILayout.Label(_selectedGroup.title, labelStyle);
					
					for(int i = 0; i < _selectedGroup.todos.Count; i++)
					{
						// Show expected drag field
						if (_expectedTodoDragIndex == i)
						{
							GUILayout.Space(20f);
						}

						// Show todo field
						Todo todo = _selectedGroup.todos[i];
						bool hasModified = false;
						if (_searchString.Length == 0 || todo.isVisible)
							TodoField(i, ref todo, ref hasModified);

						// Break if modified
						if (hasModified)
							break;
					}
					TodoAddButton();
				}
			}

			// Reset values
			if (e.isMouse && e.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(e.mousePosition))
			{
				if (_hasSelectedTodoChanged == false)
					SetSelectedTodo(null);

				if (_hasTodoDragStarted == false && _startTodoDragIndex >= 0)
					_startTodoDragIndex = -1;
			}

			_hasSelectedTodoChanged = false;
			_hasTodoDragStarted = false;
		}


		/** Toolbar Functions */
		private void TagField()
		{
			GUIStyle style = _currentTag == null ? "Label" : "Button";
			// Default tag
			if (GUILayout.Button("All", style))
				SetCurrentTag(null);

			// Show all tags
			for (int i = 0; i < _config.GetTagCount(); i++)
			{
				Tag tag = _config.GetTagByIndex(i);
				bool isCurrentTag = _currentTag == tag;
				GUI.backgroundColor = tag.color;

				if (isCurrentTag)
				{
					if (i < 2)
					{
						GUILayout.Label(tag.name);
					}
					else
					{
						tag.name = GUILayout.TextField(tag.name);

						GUIContent content = new GUIContent("-", "Remove this tag");
						if (_currentGroups.Count > 0)
						{
							content.tooltip = "Can't remove this tag because it has an item.";
							GUI.enabled = false;
						}

						// Remove tag button
						if (GUILayout.Button(content))
						{
							_config.RemoveTag(ref tag);
						}

						GUI.enabled = true;
					}
				}
				else
				{
					if (GUILayout.Button(tag.name))
						SetCurrentTag(tag);
				}
				GUI.backgroundColor = Color.white;
			}

			// Add button
			GUIContent addButtonContent = new GUIContent("+", "Create a new tag");
			if (GUILayout.Button(addButtonContent))
			{
				string defaultName = "NEW TAG";
				Tag newTag = new Tag(defaultName, Color.white, _config.GetTagCount());
				_config.AddTag(ref newTag);
			}

		}

		private void DrawSearchbar()
		{
			float searchbarWidth = Mathf.Max(Screen.width / 4f, (float)TodoLayout.MinSearchbarWidth);
			_searchString = GUILayout.TextField(_searchString, "ToolbarSeachTextField", GUILayout.Width(searchbarWidth));
            if (GUILayout.Button("", "ToolbarSeachCancelButton"))
            {
                _searchString = "";
                GUI.FocusControl(null);
            }

			_searchString = _searchString.TrimStart();
			SearchTodos();
		}

		private void SearchTodos()
		{
			// Return if no data or no need to search again
			if (_currentGroups == null || _searchString.Equals(_preSearchString))
				return;

			// Rollback to before search
			if (_searchString.Length < 1)
			{
				RefreshCurrentGroups();
				return;
			}
			
			// Search
			foreach (TodoGroup group in _currentGroups)
			{
				bool hasVisibleTodo = false;
				foreach (Todo todo in group.todos)
				{
					todo.isVisible = todo.title.ToLower().Contains(_searchString.ToLower());
					if (todo.isVisible)
						hasVisibleTodo = true;
				}
				group.isVisible = hasVisibleTodo;
			}

			// Update preSearchString
			_preSearchString = _searchString;
		}


		/** Sidebar Functions */
		private void TodoGroupField(int index)
		{
			Event e = Event.current;
			TodoGroup group = _currentGroups[index];
			GUIStyle labelStyle = EditorStyles.label;

			// Set style depends on whether selected group
			if (_selectedGroup != null)
			{
				Color backgroundColor = group.tag.color == Color.white ? Color.black : group.tag.color;
				if (_selectedGroup == _currentGroups[index])
				{
					GUI.backgroundColor = backgroundColor;
					labelStyle = EditorStyles.boldLabel;
				}
				else
					GUI.backgroundColor = Color.white;
			}

			// Show todo group
			using (new HorizontalGroup(EditorStyles.helpBox, GUILayout.Height((float)TodoLayout.TodoGroupFieldHeight)))
			{
				GUI.color = group.tag.color;

				GUILayout.Label(group.title, labelStyle, GUILayout.ExpandHeight(true));
				GUILayout.FlexibleSpace();
				GUILayout.Label("(" + group.todos.Count + ")", labelStyle, GUILayout.ExpandHeight(true));
				GUI.color = Color.white;
				GUI.backgroundColor = Color.white;
			}

			// On clicked
			if (e.isMouse && e.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(e.mousePosition))
			{
				SetSelectedGroup(index);
			}
		}

		private void DisplaySelectedTodoGroup()
		{
			if (_selectedGroup == null || _showTodoGroup == false)
				return;

			// Backup original GUI settings
			int originalTextFieldFontSize = GUI.skin.textField.fontSize;
			int originalTextAreaFontSize = GUI.skin.textArea.fontSize;

			// Change font size
			GUI.skin.textField.fontSize = 14;
			GUI.skin.textArea.fontSize = 12;

			/** Display TodoGroup */
			float height = Mathf.Max((float)TodoLayout.MinTodoGroupHeight, Screen.height / 4f);

			using (new VerticalGroup(EditorStyles.helpBox, GUILayout.Height(height)))
			{
				// Top bar
				using (new HorizontalGroup())
				{
					GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
					labelStyle.fontSize = 14;
					labelStyle.padding = new RectOffset(4, 4, 4, 4);
					labelStyle.normal.textColor = Color.white;
					GUILayout.Label("INFO", labelStyle);
					
					GUIStyle buttonStyle = new GUIStyle(GUI.skin.label);
					buttonStyle.fontSize = 16;
					buttonStyle.fontStyle = FontStyle.Bold;
					if (GUILayout.Button("x", buttonStyle, GUILayout.Width(20f)))
					{
						HideTodoGroup();
					}
				}			

				// Group contents
				_selectedGroup.title = GUILayout.TextField(_selectedGroup.title, 50, GUILayout.Height((float)TodoLayout.TodoGroupTitleHeight));

				// Tag
				using (new HorizontalGroup())
				{
					GUILayout.Label("Tag    ", GUILayout.ExpandWidth(false));
					int tagIndex = EditorGUILayout.Popup(_selectedGroup.tag.index, _config.GetTagNames());
					// Switch tag
					if (tagIndex != _selectedGroup.tag.index)
					{
						_selectedGroup.tag = _config.GetTagByIndex(tagIndex);
						RefreshCurrentGroups();
					}
					_selectedGroup.tag.color = EditorGUILayout.ColorField(_selectedGroup.tag.color, GUILayout.Width((float)TodoLayout.TagColorWidth), GUILayout.Height((float)TodoLayout.TodoGroupTitleHeight));
				}

				// Asset
				using (new HorizontalGroup())
				{
					GUILayout.Label("Asset ", GUILayout.ExpandWidth(false));
					_selectedGroup.reference = EditorGUILayout.ObjectField(_selectedGroup.reference, typeof(UnityEngine.Object), false);
				}

				// Note
				GUILayout.Space((float)TodoLayout.TodoGroupSpace);
				GUILayout.Label("Note ");
				_selectedGroup.note = GUILayout.TextArea(_selectedGroup.note, GUILayout.ExpandHeight(true));
			}

			// Rollback original GUI settings
			GUI.skin.textField.fontSize = originalTextFieldFontSize;
			GUI.skin.textArea.fontSize = originalTextAreaFontSize;
		}

		private void DisplayTodoGroupButtons()
		{
			int originalButtonFontSize = GUI.skin.button.fontSize;
			GUI.skin.button.fontSize = 20;
			GUI.skin.button.fontStyle = FontStyle.Bold;

			using (new HorizontalGroup())
			{
				TodoAddGroupField();
				TodoRemoveGroupField();
			}
			GUI.skin.button.fontSize = originalButtonFontSize;
			GUI.skin.button.fontStyle = FontStyle.Normal;
		}

		private void TodoAddGroupField()
		{
			const string defaultName = "New Asset";
			GUIContent content = new GUIContent("+", "Create a new " + (_currentTag?.name ?? "TODO") + " asset");
			// Create new group
			if (GUILayout.Button(content, GUILayout.Height((float)TodoLayout.CreateGroupButtonHeight)))
			{
				TodoGroup group = new TodoGroup(defaultName, _currentTag ?? _config.GetTagByIndex(0));
				_data.AddGroup(ref group);
				RefreshCurrentGroups();
			}
		}

		private void TodoRemoveGroupField()
		{
			if (_selectedGroup == null || _showTodoGroup == false)
				return;

			GUIContent content = new GUIContent("-", "Remove this " + _selectedGroup.tag.name + " asset");
			if (_selectedGroup.todos.Count > 0)
			{
				GUI.enabled = false;
				content.tooltip = "Can't remove this asset because it has an item.";
			}
			
			if (GUILayout.Button(content, GUILayout.Height((float)TodoLayout.CreateGroupButtonHeight)))
			{
				_data.RemoveGroup(ref _selectedGroup);
				RefreshCurrentGroups();
				SetSelectedGroup(-1);
			}
			GUI.enabled = true;
		}

		private void HideTodoGroup()
		{
			_showTodoGroup = false;
		}


		/** Todo Area Functions */
		private void TodoField(int index, ref Todo todo, ref bool hasModified)
		{
			GUIStyle fieldStyle = new GUIStyle(GUI.skin.box);
			Event e = Event.current;

			// Notify if dragging
			if (_startTodoDragIndex == index)
				GUI.backgroundColor = new Color(0.2f, 0.1f, 1f, 0.4f);

			using (new HorizontalGroup(fieldStyle, GUILayout.Height((float)TodoLayout.TodoFieldHeight)))
			{
				// Title & Description
				using (new VerticalGroup())
				{
					TodoTitleField(ref todo);
					todo.description = GUILayout.TextArea(todo.description, GUILayout.Height((float)TodoLayout.TodoFieldHeight - 22f));
				}

				// Priority & Progress
				float spaceHeight = (float)TodoLayout.TodoFieldHeight / 3f;
				using (new VerticalGroup(GUILayout.Width(Screen.width / 8f), GUILayout.MinWidth((float)TodoLayout.MinPriorityPopupWidth)))
				{
					GUILayout.Space(spaceHeight);
					TodoPriority(ref todo);
					TodoProgress(ref todo);
					GUILayout.Space(spaceHeight);
				}

				// Remove Button
				TodoRemoveButton(ref todo, spaceHeight, ref hasModified);
			}

			GUI.backgroundColor = Color.white;

			// Rearrange fields
			RearrangeTodoFields(ref e, index);
		}

		private void TodoTitleField(ref Todo todo)
		{
			Event e = Event.current;

			// Switch to textfield if clicked
			if (_selectedTodo == todo)
			{
				GUIStyle style = new GUIStyle(GUI.skin.textField);
				style.fontSize = 15;
				todo.title = GUILayout.TextField(todo.title, style, GUILayout.MinWidth((float)TodoLayout.MinTodoTitleWidth));
			}
			else
			{
				GUIStyle style = new GUIStyle(GUI.skin.label);
				style.fontStyle = FontStyle.Bold;
				style.fontSize = 15;
				GUILayout.Label(todo.title, style, GUILayout.MinWidth((float)TodoLayout.MinTodoTitleWidth));
			}

			// Update selectedTodo
			if (e.isMouse && e.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(e.mousePosition))
			{
				SetSelectedTodo(todo);
				_hasSelectedTodoChanged = true;
			}
		}

		private void TodoPriority(ref Todo todo)
		{
			GUI.color = todo.priority.color;
			int priorityIndex = EditorGUILayout.Popup(todo.priority.index, _config.GetPriorityNames());
			// Switch if modified
			if (priorityIndex != todo.priority.index)
			{
				todo.priority = _config.GetPriorityByIndex(priorityIndex);
			}
			GUI.color = Color.white;
		}

		private void TodoProgress(ref Todo todo)
		{
			GUI.color = todo.progress.color;
			int progressIndex = EditorGUILayout.Popup(todo.progress.index, _config.GetProgressStatuses());
			// Switch if modified
			if (progressIndex != todo.progress.index)
			{
				todo.progress = _config.GetProgressByIndex(progressIndex);
			}
			GUI.color = Color.white;
		}

		private void TodoRemoveButton(ref Todo todo, float spaceHeight, ref bool hasModified)
		{
			using (new VerticalGroup(GUILayout.Width(spaceHeight + 8f)))
			{
				GUILayout.Space(spaceHeight);
				GUIStyle style = new GUIStyle(GUI.skin.button);
				style.fontSize = 14;
				style.fontStyle = FontStyle.Bold;
				GUIContent content = new GUIContent("x", "Remove this " + _selectedGroup.tag.name);

				if (GUILayout.Button(content, style, GUILayout.Height(spaceHeight + 8f)))
				{
					_selectedGroup.RemoveTodo(ref todo);
					hasModified = true;
				}
				GUILayout.Space(spaceHeight);
			}
		}

		private void TodoAddButton()
		{
			int originalButtonFontSize = GUI.skin.button.fontSize;
			GUI.skin.button.fontSize = 18;
			GUI.skin.button.fontStyle = FontStyle.Bold;
			GUIContent content = new GUIContent("+", "Create a new " + _selectedGroup.tag.name);

			if (GUILayout.Button(content, GUILayout.Height((float)TodoLayout.TodoAddButtonHeight)))
			{
				string defaultName = _selectedGroup.tag.name + " " + (_selectedGroup.todos.Count + 1);
				Todo todo = new Todo(defaultName, "", _config.GetProgressByIndex(1), _config.GetPriorityByIndex(1), null);
				_selectedGroup.AddTodo(ref todo);
			}

			GUI.skin.button.fontSize = originalButtonFontSize;
			GUI.skin.button.fontStyle = FontStyle.Normal;
		}


		/** Util Functions */
		async private void SetCurrentTag(Tag tag)
		{
			await Task.Delay(10);
			_currentTag = tag;
			RefreshCurrentGroups();
			HideTodoGroup();
		}

		private void RefreshCurrentGroups()
		{
			_currentGroups = _data.GetVisibleGroups(_currentTag);
			Repaint();
		}

		async private void SetSelectedGroup(int index)
		{
			await Task.Delay(10);
			if (index < 0)
			{
				HideTodoGroup();
				_selectedGroup = null;
			}
			else
			{
				_showTodoGroup = true;
				_selectedGroup = _currentGroups[index];
			}
			SetSelectedTodo(null);
			Repaint();
		}

		private void SetSelectedTodo(Todo todo)
		{
			_selectedTodo = todo;
		}

		private void RearrangeTodoFields(ref Event e, int index)
		{
			// Return if searching
			if (_searchString.Length > 0)
				return;

			Rect fieldRect = GUILayoutUtility.GetLastRect();

			if (e.isMouse)
			{
				// Set dragging values if dragging has started
				if (e.type == EventType.MouseDown && fieldRect.Contains(e.mousePosition))
				{
					_hasTodoDragStarted = true;
					_startTodoDragIndex = index;
					_startTodoDragPosY = e.mousePosition.y;
				}

				// Update expected index if dragging
				if (_startTodoDragIndex >= 0 && e.type == EventType.MouseDrag)
				{
					_expectedTodoDragIndex = GetExpectedTodoDragIndex(e.mousePosition.y);
				}

				// Rearrange items if dragging has ended
				if (_startTodoDragIndex >= 0 && e.type == EventType.MouseUp)
				{
					_selectedGroup.RearrangeTodos(_startTodoDragIndex, GetExpectedTodoDragIndex(e.mousePosition.y));
					_startTodoDragIndex = -1;
					_expectedTodoDragIndex = -1;
				}
			}
		}

		private int GetExpectedTodoDragIndex(float mouseY)
		{
			int diff = (int)((mouseY - _startTodoDragPosY) / (float)TodoLayout.TodoFieldHeight);
			return (int)Mathf.Clamp(_startTodoDragIndex + diff, 0f, _selectedGroup.todos.Count - 1);
		}


		public void OnDisable()
		{
			if (!EditorApplication.isPlayingOrWillChangePlaymode)
			{
				SaveTodoTree();
			}
		}
		public void OnDestroy()
		{
			SaveTodoTree();
		}

		private void SaveTodoTree()
		{
			EditorUtility.SetDirty(_config);
			EditorUtility.SetDirty(_data);
			AssetDatabase.SaveAssets();
		}
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Todo
{
    [InitializeOnLoad]
    static class TodoInspectorGUI
    {
        private static GUIContent _todoAddButtonText;
        private static GUIContent _fixmeAddButtonText;
        private static TodoConfig _config;
        private static TodoDatabase _data;

        static TodoInspectorGUI()
        {
            LoadTodoConfig();
            LoadTodoDatabase();
            Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
        }

        private static void OnPostHeaderGUI(Editor editor)
        {
            if (editor.target && _config != null
                && _data != null && _config.inpsectorGUI)
            {
                if (!typeof(UnityEngine.Object).IsAssignableFrom(editor.target.GetType()))
                    return;

                if (typeof(UnityEngine.GameObject).Equals(editor.target.GetType()))
                    return;

                using (new HorizontalGroup())
                {
                    GUILayout.Label("TODO Tree", GUILayout.Width(Screen.width / 3f));
                    for (int i = 0; i < 2; i++)
                    {
                        GUIContent content = new GUIContent("New " + _config.tags[i].name);
                        if (GUILayout.Button(content))
                        {
                            TodoGroup newGroup;
                            if (editor.target.name.Trim().Length > 0)
                            {
                                newGroup = new TodoGroup(editor.target.name, _config.tags[i]);
                                newGroup.reference = editor.target;
                            }
                            else
                            {
                                string assetPath = AssetDatabase.GetAssetOrScenePath(editor.target);
                                string[] splitPath = assetPath.Split('/');
                                newGroup = new TodoGroup(splitPath[splitPath.Length - 1].Split('.')[0], _config.tags[i]);
                                newGroup.reference = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                            }
                            _data.AddGroup(ref newGroup);
                            EditorUtility.SetDirty(_data);
                            AssetDatabase.SaveAssets();
                        }
                    }
                }
            }
        }


        private static void LoadTodoConfig()
		{
			_config = AssetDatabase.LoadAssetAtPath<TodoConfig>("Assets/Editor/TodoTree/TodoConfig.asset");
		}

		private static void LoadTodoDatabase()
		{
			_data = AssetDatabase.LoadAssetAtPath<TodoDatabase>("Assets/Editor/TodoTree/TodoDatabase.asset");
		}

    }
}

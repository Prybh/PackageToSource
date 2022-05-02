using UnityEditor;
using UnityEngine;

namespace PackageToSource
{
    public class SettingsWindow : PopupWindowContent
    {
        public override Vector2 GetWindowSize()
        {
            return new Vector2(270, 150);
        }

        public override void OnGUI(Rect rect)
        {
            GUILayout.Space(5);
            GUILayout.Label("Settings", EditorStyles.boldLabel);
            GUILayout.Space(10);


            using (var toolbarScope = new GUILayout.HorizontalScope())
            {
                float previousLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 50.0f;
                Settings.gitProjectsPath = EditorGUILayout.TextField(new GUIContent("Projects", "Path to host git projects"), Settings.gitProjectsPath);
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_Project", "Show in explorer")))
                {
                    string newPath = EditorUtility.SaveFolderPanel("Git Projects Path", Settings.gitProjectsPath, "");
                    if (newPath.Length > 0)
                    {
                        Settings.gitProjectsPath = newPath;
                    }
                }
                EditorGUIUtility.labelWidth = previousLabelWidth;
            }
            GUILayout.Space(5);
            Settings.shellName = EditorGUILayout.TextField(new GUIContent("Shell", "Shell process to launch commands"), Settings.shellName);
            GUILayout.Space(5);
            Settings.deleteOnUnused = EditorGUILayout.Toggle(new GUIContent("Delete Unused Repository", "Delete unused repository"), Settings.deleteOnUnused);
            GUILayout.Space(5);
            Settings.debugLogger = EditorGUILayout.Toggle(new GUIContent("Debug", "Enable debug logs"), Settings.debugLogger);
        }
    }
}
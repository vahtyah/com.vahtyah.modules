using UnityEditor;
using UnityEngine;

namespace VahTyah
{
    [CustomEditor(typeof(EditorStyleDatabase))]
    public class EditorStyleDatabaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorStyleDatabase database = (EditorStyleDatabase)target;
            DrawDefaultInspector();
            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Default Style", GUILayout.Height(25)))
            {
                Undo.RecordObject(database, "Add Default Style");
                database.AddDefaultStyle();
                EditorUtility.SetDirty(database);
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
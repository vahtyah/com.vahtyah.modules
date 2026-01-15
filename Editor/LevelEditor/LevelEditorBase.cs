using System;
using UnityEditor;
using UnityEngine;
using VahTyah;

public abstract class LevelEditorBase : EditorWindow
{
    public static EditorWindow Window;

    private ResizableSeparator _resizableSidebar;
    private LevelsHandlerBase _levelHandler;

    [MenuItem("Tools/Level Editor")]
    static void ShowWindow()
    {
        Type childType = GetChildType();
        Window = GetWindow(childType);
        Window.titleContent = new GUIContent("Level Editor");
        Window.minSize = new Vector2(200, 200);
        Window.Show();
    }

    [MenuItem("Tools/Level Editor", true)]
    static bool ValidateMenuItem()
    {
        Type childType = GetChildType();
        return (childType != null);
    }

    static Type GetChildType()
    {
        foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type classType in assembly.GetTypes())
            {
                if (classType.IsSubclassOf(typeof(LevelEditorBase)))
                {
                    return classType;
                }
            }
        }

        return null;
    }

    protected virtual void OnEnable()
    {
        _levelHandler = GetLevelHandler;
        _resizableSidebar = new ResizableSeparator("editor_sidebar_width", 240);
    }

    protected abstract LevelsHandlerBase GetLevelHandler { get; }

    protected virtual void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(_resizableSidebar.CurrentWidth));
        _levelHandler.DisplayReordableList();
        _levelHandler.DrawToolbar();
        EditorGUILayout.EndVertical();

        _resizableSidebar.DrawResizeSeparator();

        EditorGUILayout.BeginVertical();
        DrawContent();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    protected virtual void DrawContent()
    {
    }
}
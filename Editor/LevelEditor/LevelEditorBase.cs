using System;
using UnityEditor;
using UnityEngine;
using VahTyah;
using Object = UnityEngine.Object;

public abstract class LevelEditorBase : EditorWindow
{
    public static EditorWindow Window;

    protected ResizableSeparator ResizableSidebar;
    private LevelsHandlerBase _levelHandler;
    
    private const string LEVEL_EDITOR_SCENE_PATH = "Assets/_Game/LevelEditor/Editor/Scene/LevelEditor.unity";
    private const string LEVEL_EDITOR_SCENE_NAME = "LevelEditor";

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
        ResizableSidebar = new ResizableSeparator("editor_sidebar_width", 240);
    }

    protected virtual void OnDisable()
    {
    }

    protected abstract LevelsHandlerBase GetLevelHandler { get; }

    protected virtual void OnGUI()
    {
        if (!LevelEditorUtils.IsInScene(LEVEL_EDITOR_SCENE_NAME))
        {
            DrawSceneRequiredMessage();
            return;
        }
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(ResizableSidebar.CurrentWidth));
        _levelHandler.DisplayReordableList();
        _levelHandler.DrawToolbar();
        EditorGUILayout.EndVertical();

        ResizableSidebar.DrawResizeSeparator();

        EditorGUILayout.BeginVertical();
        DrawContent();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    protected virtual void DrawContent()
    {
    }
    

    private void DrawSceneRequiredMessage()
    {
        EditorGUILayout.Space(20);
        
        EditorGUILayout.HelpBox(
            "Level Editor requires the LevelEditor scene to be open.\n\n" +
            "Please open the LevelEditor scene to use this tool.",
            MessageType.Warning
        );
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Open LevelEditor Scene", GUILayout.Height(30)))
        {
            LevelEditorUtils.OpenScene(LEVEL_EDITOR_SCENE_PATH);
        }
    }
}

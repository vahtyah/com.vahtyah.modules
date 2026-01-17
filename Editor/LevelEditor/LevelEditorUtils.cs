using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelEditorUtils
{
    private static string pendingScenePath;
    private static bool pendingAskToSave;

    #region Scene Controller

    public static bool IsInScene(string sceneName)
    {
        var activeScene = SceneManager.GetActiveScene();
        return activeScene.name == sceneName;
    }

    public static void OpenScene(string scenePath, bool askToSave = true)
    {
        if (Application.isPlaying)
        {
            pendingScenePath = scenePath;
            pendingAskToSave = askToSave;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.isPlaying = false;
            return;
        }

        OpenSceneInternal(scenePath, askToSave);
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode && !string.IsNullOrEmpty(pendingScenePath))
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            var path = pendingScenePath;
            var askToSave = pendingAskToSave;
            pendingScenePath = null;
            
            // Delay to ensure editor is fully ready
            EditorApplication.delayCall += () => OpenSceneInternal(path, askToSave);
        }
    }

    private static void OpenSceneInternal(string scenePath, bool askToSave = true)
    {
        if (askToSave && SceneManager.GetActiveScene().isDirty)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }
        }

        EditorSceneManager.OpenScene(scenePath);
    }

    #endregion
}

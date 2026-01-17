#pragma warning disable 649

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using Object = UnityEngine.Object;

namespace VahTyah
{
    public abstract class LevelsHandlerBase
    {
        #region Constants

        private const string LEVEL_PREFIX = "Level_";
        private const string ASSET_SUFFIX = ".asset";
        private const string OLD_PREFIX = "old_";
        private const string REMOVE_LEVEL = "Are you sure you want to remove ";
        private const string BRACKET = "\"";
        private const string QUESTION_MARK = "?";
        private const string REMOVING_LEVEL_TITLE = "Removing level";
        private const string YES = "Yes";
        private const string CANCEL = "Cancel";
        private const string FORMAT_TYPE = "000";
        private const string PATH_SEPARATOR = "/";
        private const string DEFAULT_LEVEL_LIST_HEADER = "Levels";
        private const string REMOVE_SELECTION = "Clear Selection";
        private const string RENAME_LEVELS_LABEL = "Rename Levels";
        private const string GLOBAL_VALIDATION_LABEL = "Global Validation";
        private const string REMOVE_ELEMENT_CALLBACK = "Delete Level";

        private const string ON_ENABLE_OVERRIDEN_ERROR =
            "LevelsHandlerBase: OnEnable overridden but levels database not assigned!";

        private const string SET_POSITION_LABEL = "Set Position... ";
        private const string DUPLICATE_LEVEL_LABEL = "Duplicate Level";
        private const string INDEX_CHANGE_WINDOW = "Change Level Position";
        private readonly Vector2 INDEX_CHANGE_WINDOW_SIZE = new Vector2(320, 80);
        
        public static bool IsLastLevelOpened { get; set; } = false;
        
        //PlayerPrefs
        private const string PREFS_LEVEL = "editor_level_index";

        #endregion

        #region Delegates

        public delegate void AddElementCallbackDelegate();

        public delegate void RemoveElementCallbackDelegate();

        public delegate void DisplayContextMenuCallbackDelegate(GenericMenu genericMenu);

        public delegate void OnClearSelectionCallbackDelegate();

        public delegate void OnRenameAllCallbackDelegate();

        public delegate void OnLevelOpenedCallbackDelegate(int levelIndex);

        public delegate void OnLevelChangedCallbackDelegate();

        public delegate void AddElementWithDropdownCallbackDelegate(Rect buttonRect);

        public AddElementCallbackDelegate addElementCallback;
        public RemoveElementCallbackDelegate removeElementCallback;
        public DisplayContextMenuCallbackDelegate displayContextMenuCallback;
        public OnClearSelectionCallbackDelegate onClearSelectionCallback;
        public OnRenameAllCallbackDelegate onRenameAllCallback;
        public OnLevelOpenedCallbackDelegate onLevelOpenedCallback;
        public OnLevelChangedCallbackDelegate onLevelChangedCallback;
        public AddElementWithDropdownCallbackDelegate addElementWithDropdownCallback;

        #endregion

        #region Fields

        private List<string> levelLabels = new();
        private Object levelsDatabase;
        private SerializedObject levelsDatabaseSerializedObject;
        private SerializedProperty levelsSerializedProperty;
        private SimpleCustomList customList;

        #endregion

        #region Properties

        public int SelectedLevelIndex => customList.SelectedIndex;

        public SerializedProperty SelectedLevelProperty
        {
            get
            {
                if (SelectedLevelIndex >= 0 && SelectedLevelIndex < levelsSerializedProperty.arraySize)
                {
                    return levelsSerializedProperty.GetArrayElementAtIndex(SelectedLevelIndex);
                }

                return null;
            }
        }

        public Object SelectedLevel
        {
            get => SelectedLevelProperty?.objectReferenceValue;
            set
            {
                if (SelectedLevelProperty != null)
                {
                    SelectedLevelProperty.objectReferenceValue = value;
                }
            }
        }

        public bool IgnoreDragEvents
        {
            get => customList.IgnoreDragEvents;
            set => customList.IgnoreDragEvents = value;
        }

        public SimpleCustomList CustomList => customList;

        public int LevelsCount => levelsSerializedProperty.arraySize;

        #endregion

        #region Constructor

        public LevelsHandlerBase()
        {
            Initialize();
        }

        private void Initialize()
        {
            SetupLevel();
            SetLevelLabels();
            SetupCustomList();
        }

        #endregion

        #region Custom List Setup

        private void SetupLevel()
        {
            levelsDatabase = EditorUtils.GetAsset(GetLevelDatabaseType);
            if (levelsDatabase != null)
            {
                levelsDatabaseSerializedObject = new SerializedObject(levelsDatabase);
                levelsSerializedProperty = levelsDatabaseSerializedObject.FindProperty(GetPropertyName);
            }
            else
            {
                Debug.LogError(ON_ENABLE_OVERRIDEN_ERROR);
            }
        }

        public abstract string GetPropertyName { get; }
        public abstract Type GetLevelDatabaseType { get; }
        public abstract Type GetLevelType { get; }

        private void SetupCustomList()
        {
            // Create list with custom label callback
            customList = new SimpleCustomList(
                levelsDatabaseSerializedObject,
                levelsSerializedProperty,
                GetLevelLabel
            );

            // Setup callbacks
            SetupCallbacks();
        }

        protected virtual void SetupCallbacks()
        {
            // Header
            customList.getHeaderLabelCallback = GetHeaderLabel;

            // Selection
            customList.selectionChangedCallback = OnSelectionChanged;

            // List changes
            customList.listChangedCallback = OnListChanged;
            customList.listReorderedCallback = OnListReordered;
            customList.listReorderedCallbackWithDetails = OnListReorderedWithDetails;

            // Add/Remove
            customList.addElementCallback = OnAddElement;
            customList.removeElementCallback = OnRemoveElement;

            // Context menu
            customList.displayContextMenuCallback = OnDisplayContextMenu;

            // Add with dropdown
            // customList.addElementWithDropdownCallback = OnAddElementWithDropdown;

            // Double-click
            customList.elementDoubleClickedCallback = OnElementDoubleClicked;

            // Search
            customList.searchFilterCallback = OnSearchFilter;

            // Undo
            customList.listUndoCallback = OnUndo;
        }

        #endregion

        #region Callbacks

        protected virtual string GetLevelLabel(SerializedProperty elementProperty, int elementIndex)
        {
            if (elementIndex >= 0 && elementIndex < levelLabels.Count)
            {
                return levelLabels[elementIndex];
            }

            return $"Level {FormatLevelNumber(elementIndex + 1)}";
        }

        private string GetHeaderLabel()
        {
            return $"{DEFAULT_LEVEL_LIST_HEADER}";
        }

        private void OnSelectionChanged()
        {
            if (SelectedLevelIndex >= 0 && SelectedLevelIndex < levelsSerializedProperty.arraySize)
            {
                OpenLevel(SelectedLevelIndex);
            }
        }

        private void OnListChanged()
        {
            levelsDatabaseSerializedObject.ApplyModifiedProperties();
            onLevelChangedCallback?.Invoke();
        }

        private void OnListReordered()
        {
            SetLevelLabels();
            AssetDatabase.SaveAssets();
        }

        private void OnListReorderedWithDetails(int fromIndex, int toIndex)
        {
            Debug.Log($"Level moved from position {FormatLevelNumber(fromIndex + 1)} to {FormatLevelNumber(toIndex + 1)}");
        }

        private void OnAddElement()
        {
            AddLevel();
            addElementCallback?.Invoke();
        }

        private void OnRemoveElement()
        {
            if (SelectedLevelIndex >= 0)
            {
                DeleteLevel(SelectedLevelIndex);
                removeElementCallback?.Invoke();
            }
        }

        private void OnDisplayContextMenu(int index)
        {
            GenericMenu menu = new GenericMenu();
            SetContextMenu(index, menu);
            displayContextMenuCallback?.Invoke(menu);

            menu.ShowAsContext();
        }

        protected virtual void SetContextMenu(int index, GenericMenu menu)
        {
            menu.AddItem(new GUIContent(SET_POSITION_LABEL), false, () => OpenSetIndexModalWindow(index));

            menu.AddSeparator("");

            menu.AddItem(new GUIContent(DUPLICATE_LEVEL_LABEL), false, () => DuplicateLevel(index));
            menu.AddItem(new GUIContent(REMOVE_ELEMENT_CALLBACK), false, () => DeleteLevel(index));
        }

        private void OnAddElementWithDropdown(Rect buttonRect)
        {
            addElementWithDropdownCallback?.Invoke(buttonRect);
        }

        protected virtual void OnElementDoubleClicked(int index)
        {
            // Double-click opens level (already handled by selection change)
            // Debug.Log($"Double-clicked level {index + 1}:  {levelLabels[index]}");
        }

        protected virtual bool OnSearchFilter(SerializedProperty prop, int index, string query)
        {
            if (index >= 0 && index < levelLabels.Count)
            {
                // Search in level label
                if (levelLabels[index].IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;

                // Search in level number
                if ((index + 1).ToString().Contains(query))
                    return true;
            }

            return false;
        }

        private void OnUndo(string undoMessage)
        {
            Undo.RecordObject(levelsDatabaseSerializedObject.targetObject, undoMessage);
        }

        #endregion

        #region Level Management

        public virtual void OpenLevel(int index)
        {
            if (index >= 0 && index < levelsSerializedProperty.arraySize)
            {
                PlayerPrefs.SetInt(PREFS_LEVEL, index);
                PlayerPrefs.Save();
                // LevelEditorBase.Instance.OpenLevel(levelObject, index);
                onLevelOpenedCallback?.Invoke(index);
            }
        }

        public void ReopenLevel()
        {
            if (SelectedLevelIndex >= 0)
            {
                OpenLevel(SelectedLevelIndex);
            }
        }

        public void AddLevel()
        {
            int newLevelIndex = levelsSerializedProperty.arraySize;
            levelsSerializedProperty.arraySize++;

            Object level = ScriptableObject.CreateInstance(GetLevelType);

            string levelNumber = GetUniqueLevelNumber();
            string assetPath = GetRelativeLevelAssetPathByNumber(levelNumber);

            AssetDatabase.CreateAsset(level, assetPath);

            string label = GetLevelLabel(level, newLevelIndex);
            levelLabels.Add(label);

            levelsSerializedProperty.GetArrayElementAtIndex(newLevelIndex).objectReferenceValue = level;
            levelsDatabaseSerializedObject.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();

            customList.SelectedIndex = newLevelIndex;

            Debug.Log($"Created new level:  {label} at {assetPath}");
        }

        public void DuplicateLevel(int sourceIndex)
        {
            if (sourceIndex < 0 || sourceIndex >= levelsSerializedProperty.arraySize)
                return;

            var sourceLevel = levelsSerializedProperty.GetArrayElementAtIndex(sourceIndex).objectReferenceValue;
            if (sourceLevel == null)
                return;

            Object newLevel = ScriptableObject.CreateInstance(GetLevelType);

            EditorUtility.CopySerialized(sourceLevel, newLevel);

            string levelNumber = GetUniqueLevelNumber();
            string assetPath = GetRelativeLevelAssetPathByNumber(levelNumber);

            string directory = Path.GetDirectoryName(assetPath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(assetPath);
            string extension = Path.GetExtension(assetPath);
            string newAssetPath = Path.Combine(directory, fileNameWithoutExtension + " (Copy)" + extension);

            AssetDatabase.CreateAsset(newLevel, newAssetPath);

            EditorUtility.SetDirty(newLevel);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            levelsSerializedProperty.arraySize++;
            int newLevelIndex = levelsSerializedProperty.arraySize - 1;

            levelsSerializedProperty.GetArrayElementAtIndex(newLevelIndex).objectReferenceValue = newLevel;

            string label = GetLevelLabel(newLevel, newLevelIndex);
            levelLabels.Add(label);

            levelsDatabaseSerializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();

            customList.SelectedIndex = newLevelIndex;
            // OpenLevel(newLevelIndex);

            Debug.Log($"Duplicated level {sourceIndex + 1} to {newLevelIndex + 1}:  {label} at {newAssetPath}");
        }

        public void DeleteLevel(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= levelsSerializedProperty.arraySize)
                return;

            // Confirmation dialog
            string levelLabel = levelIndex < levelLabels.Count ? levelLabels[levelIndex] : $"Level {FormatLevelNumber(levelIndex + 1)}";
            string message = $"{REMOVE_LEVEL}{BRACKET}{levelLabel}{BRACKET}{QUESTION_MARK}";

            if (!EditorUtility.DisplayDialog(REMOVING_LEVEL_TITLE, message, YES, CANCEL))
                return;

            // Get level object
            var levelObject = levelsSerializedProperty.GetArrayElementAtIndex(levelIndex).objectReferenceValue;

            // Delete asset
            if (levelObject != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(levelObject);
                AssetDatabase.DeleteAsset(assetPath);
                Debug.Log($"Deleted level asset: {assetPath}");
            }

            // Remove from array
            levelsSerializedProperty.GetArrayElementAtIndex(levelIndex).objectReferenceValue = null;
            levelsSerializedProperty.DeleteArrayElementAtIndex(levelIndex);

            // Remove label
            if (levelIndex < levelLabels.Count)
            {
                levelLabels.RemoveAt(levelIndex);
            }

            // Clear selection
            customList.SelectedIndex = -1;

            // Save
            levelsDatabaseSerializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Deleted level {levelIndex + 1}: {levelLabel}");
        }

        public void UpdateCurrentLevelLabel(string label)
        {
            if (SelectedLevelIndex >= 0 && SelectedLevelIndex < levelLabels.Count)
            {
                levelLabels[SelectedLevelIndex] = label;
            }
        }

        #endregion

        #region Level Naming

        public virtual void SetLevelLabels()
        {
            levelLabels.Clear();

            for (int i = 0; i < levelsSerializedProperty.arraySize; i++)
            {
                levelLabels.Add(GetLevelLabel(levelsSerializedProperty.GetArrayElementAtIndex(i).objectReferenceValue,
                    i));
            }
        }

        public virtual string GetLevelLabel(Object levelObject, int index)
        {
            return $"{FormatLevelNumber(index + 1)} | {levelObject.name}";
        }

        public void RenameLevels()
        {
            List<int> incorrectIndices = new List<int>();

            for (int i = 0; i < levelsSerializedProperty.arraySize; i++)
            {
                var levelObject = levelsSerializedProperty.GetArrayElementAtIndex(i).objectReferenceValue;
                if (levelObject == null)
                    continue;

                string expectedName = LEVEL_PREFIX + FormatNumber(i + 1);
                if (!levelObject.name.Equals(expectedName))
                {
                    incorrectIndices.Add(i);
                }
            }

            if (incorrectIndices.Count == 0)
            {
                Debug.Log("All levels already have correct names.");
                return;
            }

            // Rename to temporary names first (avoid conflicts)
            foreach (int index in incorrectIndices)
            {
                var levelObject = levelsSerializedProperty.GetArrayElementAtIndex(index).objectReferenceValue;
                if (levelObject != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(levelObject);
                    string tempName = OLD_PREFIX + levelObject.name;
                    AssetDatabase.RenameAsset(assetPath, tempName);
                }
            }

            // Rename to correct names
            foreach (int index in incorrectIndices)
            {
                var levelObject = levelsSerializedProperty.GetArrayElementAtIndex(index).objectReferenceValue;
                if (levelObject != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(levelObject);
                    string correctName = LEVEL_PREFIX + FormatNumber(index + 1);
                    AssetDatabase.RenameAsset(assetPath, correctName);
                }
            }

            // Save and refresh
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Update labels
            SetLevelLabels();

            onRenameAllCallback?.Invoke();

            Debug.Log($"Renamed {incorrectIndices.Count} levels to correct format.");
        }

        #endregion

        #region Validation

        public virtual void GlobalValidation()
        {
            Debug.Log("=== Global Validation Started ===");

            int errorCount = 0;

            for (int i = 0; i < levelsSerializedProperty.arraySize; i++)
            {
                var levelObject = levelsSerializedProperty.GetArrayElementAtIndex(i).objectReferenceValue;

                if (levelObject == null)
                {
                    Debug.LogError($"Level {FormatLevelNumber(i + 1)}:  NULL reference!");
                    errorCount++;
                    continue;
                }

                Debug.Log($"Validating Level {FormatLevelNumber(i + 1)}: {levelLabels[i]}");
                // LevelEditorBase.Instance.LogErrorsForGlobalValidation(levelObject, i);
            }

            SetLevelLabels();

            Debug.Log($"=== Global Validation Completed ({errorCount} errors found) ===");
        }

        #endregion

        #region Display

        public void DisplayReordableList()
        {
            OpenLastActiveLevel();
            customList.Display();
        }

        #endregion

        #region UI Buttons

        public void DrawRenameLevelsButton()
        {
            if (GUILayout.Button(RENAME_LEVELS_LABEL, UnityEditor.EditorStyles.miniButton))
            {
                RenameLevels();
            }
        }

        public void DrawGlobalValidationButton()
        {
            if (GUILayout.Button(GLOBAL_VALIDATION_LABEL, UnityEditor.EditorStyles.miniButton))
            {
                GlobalValidation();
            }
        }

        public void DrawToolbar()
        {
            EditorGUILayout.BeginVertical();
            DrawButtonToolbar();
            EditorGUILayout.EndVertical();
        }

        protected virtual void DrawButtonToolbar()
        {
            DrawRenameLevelsButton();
            DrawGlobalValidationButton();
        }

        #endregion

        #region Helpers

        public abstract string LevelFolderPath { get; }

        public void OpenLastActiveLevel()
        {
            if (!IsLastLevelOpened && (levelsSerializedProperty.arraySize > 0) && PlayerPrefs.HasKey(PREFS_LEVEL))
            {
                CustomList.SelectedIndex = Mathf.Clamp(PlayerPrefs.GetInt(PREFS_LEVEL, 0), 0,
                    levelsSerializedProperty.arraySize - 1);
                IsLastLevelOpened = true;
                ReopenLevel();
            }
        }

        private string GetUniqueLevelNumber()
        {
            int levelNumber = levelsSerializedProperty.arraySize;

            while (true)
            {
                string formattedNumber = FormatNumber(levelNumber);
                string path = Application.dataPath.Replace("Assets", string.Empty) +
                              GetRelativeLevelAssetPathByNumber(formattedNumber);

                if (!File.Exists(path))
                {
                    return formattedNumber;
                }

                levelNumber++;
            }
        }

        private string GetRelativeLevelAssetPathByNumber(string levelNumber)
        {
            return LevelFolderPath + PATH_SEPARATOR + LEVEL_PREFIX + levelNumber + ASSET_SUFFIX;
        }

        private static string FormatNumber(int number)
        {
            return number.ToString(FORMAT_TYPE);
        }

        private string FormatLevelNumber(int number)
        {
            int maxNumber = levelsSerializedProperty.arraySize;
            int digits = maxNumber.ToString().Length;
            string format = new string('0', digits);
            return number.ToString(format).PadRight(digits);
        }

        #endregion

        #region Set Index Modal Window

        private void OpenSetIndexModalWindow(int currentIndex)
        {
            SetIndexModalWindow window = ScriptableObject.CreateInstance<SetIndexModalWindow>();
            window.SetData(currentIndex, levelsSerializedProperty.arraySize, this);
            window.minSize = INDEX_CHANGE_WINDOW_SIZE;
            window.maxSize = INDEX_CHANGE_WINDOW_SIZE;
            window.titleContent = new GUIContent(INDEX_CHANGE_WINDOW);
            window.ShowModal();
        }

        private void MoveLevel(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex)
                return;

            levelsSerializedProperty.MoveArrayElement(fromIndex, toIndex);
            levelsDatabaseSerializedObject.ApplyModifiedProperties();

            customList.SelectedIndex = toIndex;

            SetLevelLabels();
            AssetDatabase.SaveAssets();

            Debug.Log($"Moved level from position {FormatLevelNumber(fromIndex + 1)} to {FormatLevelNumber(toIndex + 1)}");
        }

        #endregion

        #region Modal Window

        private class SetIndexModalWindow : EditorWindow
        {
            private const string LABEL_FORMAT = "Level #{0}";
            private const string TARGET_POSITION_LABEL = "New Position (1-{0})";
            private const string CANCEL_BUTTON_LABEL = "Cancel";
            private const string MOVE_BUTTON_LABEL = "Move";

            private int originalIndex;
            private int arraySize;
            private int newPosition;
            private LevelsHandlerBase _levelsHandlerBase;

            public void SetData(int originalIndex, int arraySize, LevelsHandlerBase levelsHandlerBase)
            {
                this.originalIndex = originalIndex;
                this.arraySize = arraySize;
                _levelsHandlerBase = levelsHandlerBase;
                newPosition = originalIndex + 1; // Convert to 1-based
            }

            private void OnGUI()
            {
                EditorGUILayout.Space(5);

                // Current level info
                EditorGUILayout.LabelField(string.Format(LABEL_FORMAT, originalIndex + 1), UnityEditor.EditorStyles.boldLabel);

                EditorGUILayout.Space(5);

                // Target position field
                EditorGUILayout.LabelField(string.Format(TARGET_POSITION_LABEL, arraySize));
                newPosition = EditorGUILayout.IntSlider(newPosition, 1, arraySize);

                EditorGUILayout.Space(10);

                // Buttons
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(CANCEL_BUTTON_LABEL))
                {
                    Close();
                }

                GUI.enabled = (newPosition - 1) != originalIndex;

                if (GUILayout.Button(MOVE_BUTTON_LABEL))
                {
                    _levelsHandlerBase.MoveLevel(originalIndex, newPosition - 1);
                    Close();
                }

                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);
            }
        }

        #endregion
    }
}

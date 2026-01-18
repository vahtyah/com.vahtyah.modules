using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VahTyah
{
    public class AssetRefDrawer
    {
        private const float BUTTON_WIDTH = 25f;

        public void DrawProperty(Rect rect, SerializedProperty property, AssetRefAttribute attribute, FieldInfo fieldInfo, Object target)
        {
            Rect fieldRect = new Rect(rect.x, rect.y, rect.width - BUTTON_WIDTH - 2, rect.height);
            Rect buttonRect = new Rect(rect.xMax - BUTTON_WIDTH, rect.y, BUTTON_WIDTH, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(fieldRect, property, new GUIContent(property.displayName), true);

            bool hasValue = property.objectReferenceValue != null;

            using (new EditorGUI.DisabledScope(hasValue))
            {
                GUIContent buttonContent = EditorGUIUtility.IconContent("Refresh");
                buttonContent.tooltip = GetTooltip(attribute);

                if (GUI.Button(buttonRect, buttonContent, EditorStyles.miniButton))
                {
                    FindAndAssign(property, attribute, fieldInfo, silent: false);
                }
            }
        }

        public bool TryAutoAssign(SerializedProperty property, AssetRefAttribute attribute, FieldInfo fieldInfo, bool silent = true)
        {
            if (property.objectReferenceValue != null)
                return false;

            return FindAndAssign(property, attribute, fieldInfo, silent);
        }

        private bool FindAndAssign(SerializedProperty property, AssetRefAttribute attribute, FieldInfo fieldInfo, bool silent = false)
        {
            Type fieldType = fieldInfo.FieldType;
            Object foundAsset = FindAsset(fieldType, attribute);

            if (foundAsset != null)
            {
                property.objectReferenceValue = foundAsset;
                property.serializedObject.ApplyModifiedProperties();
                return true;
            }

            if (!silent)
            {
                string searchInfo = string.IsNullOrEmpty(attribute.AssetName) 
                    ? $"type {fieldType.Name}" 
                    : $"name '{attribute.AssetName}'";
                Debug.LogWarning($"[AssetRef] Could not find asset with {searchInfo}");
            }
            return false;
        }

        private Object FindAsset(Type fieldType, AssetRefAttribute attribute)
        {
            string typeFilter = GetTypeFilter(fieldType);
            string searchQuery = BuildSearchQuery(attribute, typeFilter);
            string[] searchFolders = GetSearchFolders(attribute);

            string[] guids = searchFolders != null
                ? AssetDatabase.FindAssets(searchQuery, searchFolders)
                : AssetDatabase.FindAssets(searchQuery);

            if (guids.Length == 0)
                return null;

            // If searching by name, find exact match first
            if (!string.IsNullOrEmpty(attribute.AssetName))
            {
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                    
                    if (fileName.Equals(attribute.AssetName, StringComparison.OrdinalIgnoreCase))
                    {
                        Object asset = AssetDatabase.LoadAssetAtPath(path, fieldType);
                        if (asset != null) return asset;
                    }
                }
            }

            // Return first matching asset
            string firstPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath(firstPath, fieldType);
        }

        private string GetTypeFilter(Type fieldType)
        {
            // For specific types, use their exact type name
            if (fieldType == typeof(GameObject))
                return "t:Prefab";
            if (fieldType == typeof(Material))
                return "t:Material";
            if (fieldType == typeof(Sprite))
                return "t:Sprite";
            if (fieldType == typeof(Texture2D))
                return "t:Texture2D";
            if (fieldType == typeof(Texture))
                return "t:Texture";
            if (fieldType == typeof(AudioClip))
                return "t:AudioClip";
            if (fieldType == typeof(AnimationClip))
                return "t:AnimationClip";
            if (fieldType == typeof(RuntimeAnimatorController))
                return "t:AnimatorController";
            
            // For ScriptableObject subclasses and other types, use specific type name
            return $"t:{fieldType.Name}";
        }

        private string BuildSearchQuery(AssetRefAttribute attribute, string typeFilter)
        {
            if (!string.IsNullOrEmpty(attribute.AssetName))
            {
                return $"{attribute.AssetName} {typeFilter}";
            }
            return typeFilter;
        }

        private string[] GetSearchFolders(AssetRefAttribute attribute)
        {
            if (!string.IsNullOrEmpty(attribute.AssetPath))
            {
                return new[] { attribute.AssetPath };
            }
            return null;
        }

        private string GetTooltip(AssetRefAttribute attribute)
        {
            if (!string.IsNullOrEmpty(attribute.AssetName) && !string.IsNullOrEmpty(attribute.AssetPath))
            {
                return $"Find asset '{attribute.AssetName}' in {attribute.AssetPath}";
            }
            if (!string.IsNullOrEmpty(attribute.AssetName))
            {
                return $"Find asset '{attribute.AssetName}'";
            }
            if (!string.IsNullOrEmpty(attribute.AssetPath))
            {
                return $"Find first asset in {attribute.AssetPath}";
            }
            return "Find first matching asset in project";
        }
    }
}

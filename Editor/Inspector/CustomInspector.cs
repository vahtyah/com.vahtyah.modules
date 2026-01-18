#if VAHTYAH_CUSTOM_INSPECTOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace VahTyah
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class MonoBehaviorInspector : CustomInspector
    {
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScriptableObject), true)]
    public class ScriptableObjectInspector : CustomInspector
    {
    }

    public class CustomInspector : Editor
    {
        private List<Type> nestedClassTypes;

        private List<PropertyGroup> propertyGroups;
        private List<SerializedProperty> ungroupedProperties;
        private Dictionary<string, (FieldInfo field, AutoRefAttribute attr)> autoRefProperties;
        private Dictionary<string, (FieldInfo field, AssetRefAttribute attr)> assetRefProperties;
        private SerializedProperty scriptProperty;

        private List<(MethodInfo method, ButtonAttribute attribute)> buttonMethods;
        private ButtonDrawer buttonDrawer;
        private AutoRefDrawer autoRefDrawer;
        private AssetRefDrawer assetRefDrawer;

        protected virtual void OnEnable()
        {
            if (target == null) return;

            scriptProperty = serializedObject.FindProperty("m_Script");

            nestedClassTypes = GetClassNestedTypes(target.GetType());

            CollectProperties();
            CollectButtonMethods();
            InspectorStyle.EnsureStyleDatabaseExists();

            buttonDrawer = new ButtonDrawer();
            autoRefDrawer = new AutoRefDrawer();
            assetRefDrawer = new AssetRefDrawer();

            // Auto-assign null references on enable
            AutoAssignNullReferences();
        }

        private void CollectProperties()
        {
            propertyGroups = new List<PropertyGroup>();
            ungroupedProperties = new List<SerializedProperty>();
            autoRefProperties = new Dictionary<string, (FieldInfo, AutoRefAttribute)>();
            assetRefProperties = new Dictionary<string, (FieldInfo, AssetRefAttribute)>();

            Dictionary<string, PropertyGroup> groupsDict = new Dictionary<string, PropertyGroup>();

            SerializedProperty iterator = serializedObject.GetIterator();

            if (iterator.NextVisible(true))
            {
                do
                {
                    if (iterator.name.Equals("m_Script", StringComparison.OrdinalIgnoreCase))
                        continue;

                    FieldInfo fieldInfo = GetFieldFromNestedTypes(iterator.name);
                    if (fieldInfo == null)
                    {
                        SerializedProperty property = serializedObject.FindProperty(iterator.propertyPath);
                        ungroupedProperties.Add(property);
                        continue;
                    }

                    GroupAttribute groupAttr = fieldInfo.GetCustomAttribute<GroupAttribute>();
                    AutoRefAttribute autoRefAttr = fieldInfo.GetCustomAttribute<AutoRefAttribute>();
                    AssetRefAttribute assetRefAttr = fieldInfo.GetCustomAttribute<AssetRefAttribute>();

                    SerializedProperty targetProperty = serializedObject.FindProperty(iterator.propertyPath);

                    // Store AutoRef attribute if present
                    if (autoRefAttr != null)
                    {
                        autoRefProperties[iterator.propertyPath] = (fieldInfo, autoRefAttr);
                    }

                    // Store AssetRef attribute if present
                    if (assetRefAttr != null)
                    {
                        assetRefProperties[iterator.propertyPath] = (fieldInfo, assetRefAttr);
                    }

                    if (groupAttr != null)
                    {
                        if (!groupsDict.TryGetValue(groupAttr.ID, out PropertyGroup group))
                        {
                            IGroupDrawer drawer = GroupDrawerRegistry.GetDrawer(groupAttr);
                            group = new PropertyGroup(groupAttr, drawer);
                            groupsDict[groupAttr.ID] = group;
                            propertyGroups.Add(group);
                        }

                        group.Properties.Add(targetProperty);
                        
                        // Also add AutoRef to group
                        if (autoRefAttr != null)
                        {
                            group.AddAutoRefAttribute(iterator.propertyPath, fieldInfo, autoRefAttr);
                        }

                        // Also add AssetRef to group
                        if (assetRefAttr != null)
                        {
                            group.AddAssetRefAttribute(iterator.propertyPath, fieldInfo, assetRefAttr);
                        }
                    }
                    else
                    {
                        ungroupedProperties.Add(targetProperty);
                    }
                } while (iterator.NextVisible(false));
            }

            propertyGroups = propertyGroups.OrderBy(g => g.Attribute.Order).ToList();
        }

        private void AutoAssignNullReferences()
        {
            bool changed = false;

            // AutoRef (components)
            if (autoRefDrawer != null)
            {
                foreach (var kvp in autoRefProperties)
                {
                    SerializedProperty property = serializedObject.FindProperty(kvp.Key);
                    if (property != null && property.objectReferenceValue == null)
                    {
                        if (autoRefDrawer.TryAutoAssign(property, kvp.Value.attr, kvp.Value.field, target))
                        {
                            changed = true;
                        }
                    }
                }
            }

            // AssetRef (assets)
            if (assetRefDrawer != null)
            {
                foreach (var kvp in assetRefProperties)
                {
                    SerializedProperty property = serializedObject.FindProperty(kvp.Key);
                    if (property != null && property.objectReferenceValue == null)
                    {
                        if (assetRefDrawer.TryAutoAssign(property, kvp.Value.attr, kvp.Value.field))
                        {
                            changed = true;
                        }
                    }
                }
            }

            if (changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        public override void OnInspectorGUI()
        {
            if (scriptProperty != null)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(scriptProperty);
                }
            }

            serializedObject.Update();

            if (ungroupedProperties != null && ungroupedProperties.Count > 0)
            {
                foreach (var property in ungroupedProperties)
                {
                    DrawPropertyWithAutoRef(property);
                }
            }

            foreach (var group in propertyGroups)
            {
                group.SetAutoRefDrawer(autoRefDrawer, target);
                group.SetAssetRefDrawer(assetRefDrawer);
                group.Draw();
            }

            DrawButtons();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPropertyWithAutoRef(SerializedProperty property)
        {
            // Check AutoRef first
            if (autoRefProperties.TryGetValue(property.propertyPath, out var autoRefInfo))
            {
                float height = EditorGUI.GetPropertyHeight(property, true);
                Rect rect = GUILayoutUtility.GetRect(0, height, GUILayout.ExpandWidth(true));
                autoRefDrawer.DrawProperty(rect, property, autoRefInfo.attr, autoRefInfo.field, target);
                return;
            }

            // Check AssetRef
            if (assetRefProperties.TryGetValue(property.propertyPath, out var assetRefInfo))
            {
                float height = EditorGUI.GetPropertyHeight(property, true);
                Rect rect = GUILayoutUtility.GetRect(0, height, GUILayout.ExpandWidth(true));
                assetRefDrawer.DrawProperty(rect, property, assetRefInfo.attr, assetRefInfo.field, target);
                return;
            }

            // Default
            EditorGUILayout.PropertyField(property, true);
        }

        private void CollectButtonMethods()
        {
            buttonMethods = new List<(MethodInfo, ButtonAttribute)>();

            const BindingFlags flags = BindingFlags.Instance |
                                       BindingFlags.Public |
                                       BindingFlags.NonPublic;

            foreach (Type type in nestedClassTypes)
            {
                MethodInfo[] methods = type.GetMethods(flags);
                foreach (MethodInfo method in methods)
                {
                    ButtonAttribute buttonAttr = method.GetCustomAttribute<ButtonAttribute>();
                    if (buttonAttr != null)
                    {
                        buttonMethods.Add((method, buttonAttr));
                    }
                }
            }

            buttonMethods = buttonMethods.OrderBy(b => b.Item2.Order).ToList();
        }

        private void DrawButtons()
        {
            if (buttonMethods == null || buttonMethods.Count == 0) return;
            if (buttonDrawer == null) buttonDrawer = new ButtonDrawer();

            EditorGUILayout.Space(5);

            for (var i = 0; i < buttonMethods.Count; i++)
            {
                var buttonTuple = buttonMethods[i];
                var method = buttonTuple.Item1;
                var attribute = buttonTuple.Item2;

                buttonDrawer.DrawButton(method, attribute, targets);

                if (i < buttonMethods.Count - 1)
                {
                    buttonDrawer.DrawSpacing();
                }
            }
        }

        #region Reflection Helpers

        private List<Type> GetClassNestedTypes(Type type)
        {
            List<Type> typesList = new List<Type> { type };

            Type currentType = type;
            while (currentType.BaseType != null)
            {
                currentType = currentType.BaseType;
                typesList.Add(currentType);
            }

            return typesList;
        }

        private FieldInfo GetFieldFromNestedTypes(string fieldName)
        {
            const BindingFlags flags = BindingFlags.Instance |
                                       BindingFlags.Public |
                                       BindingFlags.NonPublic;

            foreach (Type type in nestedClassTypes)
            {
                FieldInfo fieldInfo = type.GetField(fieldName, flags);
                if (fieldInfo != null)
                    return fieldInfo;
            }

            return null;
        }

        #endregion
    }
}
#endif

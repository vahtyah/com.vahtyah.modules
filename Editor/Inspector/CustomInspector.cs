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
        private Dictionary<string, (FieldInfo field, OnValueChangedAttribute[] attrs, MethodInfo[] methods)> onValueChangedProperties;
        private SerializedProperty scriptProperty;

        private List<(MethodInfo method, ButtonAttribute attribute)> buttonMethods;
        private ButtonDrawer buttonDrawer;
        private AutoRefDrawer autoRefDrawer;
        private AssetRefDrawer assetRefDrawer;
        private OnValueChangedHandler onValueChangedHandler;
        private OnValueChangedDrawer onValueChangedDrawer;

        protected virtual void OnEnable()
        {
            if (target == null) return;

            scriptProperty = serializedObject.FindProperty("m_Script");

            nestedClassTypes = GetClassNestedTypes(target.GetType());

            // Initialize handlers before collecting properties
            buttonDrawer = new ButtonDrawer();
            autoRefDrawer = new AutoRefDrawer();
            assetRefDrawer = new AssetRefDrawer();
            onValueChangedHandler = new OnValueChangedHandler(targets, serializedObject);
            onValueChangedDrawer = new OnValueChangedDrawer();

            CollectProperties();
            CollectButtonMethods();
            InspectorStyle.EnsureStyleDatabaseExists();

            // Auto-assign null references on enable
            AutoAssignNullReferences();
        }

        private void CollectProperties()
        {
            propertyGroups = new List<PropertyGroup>();
            ungroupedProperties = new List<SerializedProperty>();
            autoRefProperties = new Dictionary<string, (FieldInfo, AutoRefAttribute)>();
            assetRefProperties = new Dictionary<string, (FieldInfo, AssetRefAttribute)>();
            onValueChangedProperties = new Dictionary<string, (FieldInfo, OnValueChangedAttribute[], MethodInfo[])>();

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
                    OnValueChangedAttribute[] onValueChangedAttrs = fieldInfo.GetCustomAttributes<OnValueChangedAttribute>().ToArray();

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

                    // Register OnValueChanged if present
                    if (onValueChangedAttrs.Length > 0)
                    {
                        MethodInfo[] methods = onValueChangedHandler.RegisterField(iterator.propertyPath, fieldInfo, onValueChangedAttrs, target.GetType());
                        onValueChangedProperties[iterator.propertyPath] = (fieldInfo, onValueChangedAttrs, methods);
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

                        // Also add OnValueChanged to group
                        if (onValueChangedAttrs.Length > 0 && onValueChangedProperties.TryGetValue(iterator.propertyPath, out var ovcInfo))
                        {
                            group.AddOnValueChangedAttribute(iterator.propertyPath, fieldInfo, ovcInfo.attrs, ovcInfo.methods);
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

            // Capture values before drawing (for OnValueChanged)
            onValueChangedHandler?.CaptureCurrentValues();

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
                group.SetOnValueChangedDrawer(onValueChangedDrawer, targets);
                group.Draw();
            }

            DrawButtons();

            if (serializedObject.ApplyModifiedProperties())
            {
                // Values were modified - check for callbacks
                onValueChangedHandler?.CheckAndInvokeCallbacks();
            }
        }

        private void DrawPropertyWithAutoRef(SerializedProperty property)
        {
            bool hasAutoRef = autoRefProperties.TryGetValue(property.propertyPath, out var autoRefInfo);
            bool hasAssetRef = assetRefProperties.TryGetValue(property.propertyPath, out var assetRefInfo);
            bool hasOnValueChanged = onValueChangedProperties.TryGetValue(property.propertyPath, out var onValueChangedInfo);

            int buttonCount = (hasAutoRef ? 1 : 0) + (hasAssetRef ? 1 : 0) + (hasOnValueChanged ? 1 : 0);

            if (buttonCount == 0)
            {
                EditorGUILayout.PropertyField(property, true);
                return;
            }

            const float BUTTON_WIDTH = 25f;
            const float BUTTON_SPACING = 2f;
            float totalButtonWidth = buttonCount * BUTTON_WIDTH + (buttonCount - 1) * BUTTON_SPACING;

            float height = EditorGUI.GetPropertyHeight(property, true);
            Rect rect = GUILayoutUtility.GetRect(0, height, GUILayout.ExpandWidth(true));

            Rect fieldRect = new Rect(rect.x, rect.y, rect.width - totalButtonWidth - 2, rect.height);
            EditorGUI.PropertyField(fieldRect, property, new GUIContent(property.displayName), true);

            float buttonX = rect.xMax - totalButtonWidth;

            if (hasAutoRef)
            {
                Rect buttonRect = new Rect(buttonX, rect.y, BUTTON_WIDTH, EditorGUIUtility.singleLineHeight);
                autoRefDrawer.DrawButton(buttonRect, property, autoRefInfo.attr, autoRefInfo.field, target);
                buttonX += BUTTON_WIDTH + BUTTON_SPACING;
            }

            if (hasAssetRef)
            {
                Rect buttonRect = new Rect(buttonX, rect.y, BUTTON_WIDTH, EditorGUIUtility.singleLineHeight);
                assetRefDrawer.DrawButton(buttonRect, property, assetRefInfo.attr, assetRefInfo.field, target);
                buttonX += BUTTON_WIDTH + BUTTON_SPACING;
            }

            if (hasOnValueChanged)
            {
                Rect buttonRect = new Rect(buttonX, rect.y, BUTTON_WIDTH, EditorGUIUtility.singleLineHeight);
                onValueChangedDrawer.DrawButton(buttonRect, property, onValueChangedInfo.attrs, onValueChangedInfo.methods, targets);
            }
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

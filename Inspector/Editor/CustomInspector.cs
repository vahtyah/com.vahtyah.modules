﻿using System;
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
        private SerializedProperty scriptProperty;

        private List<(MethodInfo method, ButtonAttribute attribute)> buttonMethods;
        private ButtonDrawer buttonDrawer;

        protected virtual void OnEnable()
        {
            if (target == null) return;

            scriptProperty = serializedObject.FindProperty("m_Script");

            nestedClassTypes = GetClassNestedTypes(target.GetType());

            CollectProperties();
            CollectButtonMethods();
            EditorStyles.EnsureStyleDatabaseExists();
            
            buttonDrawer = new ButtonDrawer();
        }

        private void CollectProperties()
        {
            propertyGroups = new List<PropertyGroup>();
            ungroupedProperties = new List<SerializedProperty>();

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

                    SerializedProperty targetProperty = serializedObject.FindProperty(iterator.propertyPath);

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
                    }
                    else
                    {
                        ungroupedProperties.Add(targetProperty);
                    }
                } while (iterator.NextVisible(false));
            }

            propertyGroups = propertyGroups.OrderBy(g => g.Attribute.Order).ToList();
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

            foreach (var property in ungroupedProperties)
            {
                EditorGUILayout.PropertyField(property, true);
            }

            foreach (var group in propertyGroups)
            {
                group.Draw();
            }

            DrawButtons();

            serializedObject.ApplyModifiedProperties();
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
                        // Validate that the method has no parameters
                        if (method.GetParameters().Length == 0)
                        {
                            buttonMethods.Add((method, buttonAttr));
                        }
                        else
                        {
                            Debug.LogWarning($"Method '{method.Name}' has ButtonAttribute but requires parameters. Only parameterless methods are supported.");
                        }
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
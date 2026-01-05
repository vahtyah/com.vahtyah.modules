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
        private List<PropertyGroup> propertyGroups;
        private List<SerializedProperty> ungroupedProperties;
        private SerializedProperty scriptProperty;

        // Nested types cache (includes base classes)
        private List<Type> nestedClassTypes;

        protected virtual void OnEnable()
        {
            if (target == null) return;

            scriptProperty = serializedObject.FindProperty("m_Script");

            // Cache nested types (current class + all base classes)
            nestedClassTypes = GetClassNestedTypes(target.GetType());

            CollectProperties();
            EditorStyles.EnsureStyleDatabaseExists();
        }

        private void CollectProperties()
        {
            propertyGroups = new List<PropertyGroup>();
            ungroupedProperties = new List<SerializedProperty>();

            Dictionary<string, PropertyGroup> groupsDict = new Dictionary<string, PropertyGroup>();

            // Iterate through serialized properties
            SerializedProperty iterator = serializedObject.GetIterator();

            if (iterator.NextVisible(true))
            {
                do
                {
                    // Skip script property
                    if (iterator.name.Equals("m_Script", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Find the field info from nested types (includes base classes)
                    FieldInfo fieldInfo = GetFieldFromNestedTypes(iterator.name);
                    if (fieldInfo == null)
                    {
                        // Field not found, draw as ungrouped
                        SerializedProperty property = serializedObject.FindProperty(iterator.propertyPath);
                        ungroupedProperties.Add(property);
                        continue;
                    }

                    // Check for GroupAttribute
                    GroupAttribute groupAttr = fieldInfo.GetCustomAttribute<GroupAttribute>();

                    SerializedProperty targetProperty = serializedObject.FindProperty(iterator.propertyPath);

                    if (groupAttr != null)
                    {
                        // Add to group
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
                        // No group - draw normally
                        ungroupedProperties.Add(targetProperty);
                    }
                } while (iterator.NextVisible(false));
            }

            // Sort groups by order
            propertyGroups = propertyGroups.OrderBy(g => g.Attribute.Order).ToList();
        }

        public override void OnInspectorGUI()
        {
            // Draw script field
            if (scriptProperty != null)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(scriptProperty);
                }
            }

            serializedObject.Update();

            // Draw ungrouped properties first
            foreach (var property in ungroupedProperties)
            {
                EditorGUILayout.PropertyField(property, true);
            }

            // Draw grouped properties
            foreach (var group in propertyGroups)
            {
                group.Draw();
            }

            serializedObject.ApplyModifiedProperties();
        }

        #region Reflection Helpers

        /// <summary>
        /// Get all types in inheritance hierarchy (current class + all base classes)
        /// This is what code gốc calls "nested types"
        /// </summary>
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

        /// <summary>
        /// Find field from current class or any base class
        /// </summary>
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
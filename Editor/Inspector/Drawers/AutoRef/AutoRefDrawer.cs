using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VahTyah
{
    public class AutoRefDrawer
    {
        private const float BUTTON_WIDTH = 25f;

        public void DrawProperty(Rect rect, SerializedProperty property, AutoRefAttribute attribute, FieldInfo fieldInfo, Object target)
        {
            Rect fieldRect = new Rect(rect.x, rect.y, rect.width - BUTTON_WIDTH - 2, rect.height);
            Rect buttonRect = new Rect(rect.xMax - BUTTON_WIDTH, rect.y, BUTTON_WIDTH, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(fieldRect, property, new GUIContent(property.displayName), true);
            DrawButton(buttonRect, property, attribute, fieldInfo, target);
        }

        public void DrawButton(Rect buttonRect, SerializedProperty property, AutoRefAttribute attribute, FieldInfo fieldInfo, Object target)
        {
            bool hasValue = property.objectReferenceValue != null;

            using (new EditorGUI.DisabledScope(hasValue))
            {
                GUIContent buttonContent = EditorGUIUtility.IconContent("Refresh");
                buttonContent.tooltip = GetTooltip(attribute);

                GUIStyle centeredButton = new GUIStyle(EditorStyles.miniButton)
                {
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(0, 0, 0, 0)
                };

                if (GUI.Button(buttonRect, buttonContent, centeredButton))
                {
                    FindAndAssign(property, attribute, fieldInfo, target, silent: false);
                }
            }
        }

        public bool TryAutoAssign(SerializedProperty property, AutoRefAttribute attribute, FieldInfo fieldInfo, Object target, bool silent = true)
        {
            if (property.objectReferenceValue != null)
                return false;

            return FindAndAssign(property, attribute, fieldInfo, target, silent);
        }

        private bool FindAndAssign(SerializedProperty property, AutoRefAttribute attribute, FieldInfo fieldInfo, Object target, bool silent = false)
        {
            if (target is not Component component)
                return false;

            Type fieldType = fieldInfo.FieldType;
            Object foundObject = FindReference(component, fieldType, attribute);

            if (foundObject != null)
            {
                property.objectReferenceValue = foundObject;
                property.serializedObject.ApplyModifiedProperties();
                return true;
            }

            if (!silent)
            {
                Debug.LogWarning($"[AutoRef] Could not find {fieldType.Name} from {attribute.Source} on {target.name}");
            }
            return false;
        }

        private Object FindReference(Component component, Type fieldType, AutoRefAttribute attribute)
        {
            return attribute.Source switch
            {
                RefSource.Self => GetComponentSelf(component, fieldType),
                RefSource.Children => GetComponentInChildren(component, fieldType, attribute.IncludeInactive),
                RefSource.Parent => GetComponentInParent(component, fieldType, attribute.IncludeInactive),
                RefSource.Scene => FindObjectOfType(fieldType, attribute.IncludeInactive),
                _ => null
            };
        }

        private Object GetComponentSelf(Component component, Type type)
        {
            // Handle GameObject field type
            if (type == typeof(GameObject))
                return component.gameObject;

            return component.GetComponent(type);
        }

        private Object GetComponentInChildren(Component component, Type type, bool includeInactive)
        {
            if (type == typeof(GameObject))
                return component.gameObject;

            // Use reflection to call generic method
            var method = typeof(Component).GetMethod("GetComponentInChildren", new[] { typeof(bool) });
            var generic = method.MakeGenericMethod(type);
            return generic.Invoke(component, new object[] { includeInactive }) as Object;
        }

        private Object GetComponentInParent(Component component, Type type, bool includeInactive)
        {
            if (type == typeof(GameObject))
                return component.transform.root.gameObject;

            var method = typeof(Component).GetMethod("GetComponentInParent", new[] { typeof(bool) });
            var generic = method.MakeGenericMethod(type);
            return generic.Invoke(component, new object[] { includeInactive }) as Object;
        }

        private Object FindObjectOfType(Type type, bool includeInactive)
        {
            if (type == typeof(GameObject))
                return null; // Can't find generic GameObject in scene

#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType(type, includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude);
#else
            return Object.FindObjectOfType(type, includeInactive);
#endif
        }

        private string GetTooltip(AutoRefAttribute attribute)
        {
            string source = attribute.Source switch
            {
                RefSource.Self => "GetComponent",
                RefSource.Children => "GetComponentInChildren",
                RefSource.Parent => "GetComponentInParent",
                RefSource.Scene => "FindObjectOfType",
                _ => "Auto"
            };

            string inactive = attribute.IncludeInactive ? " (include inactive)" : "";
            return $"Auto find: {source}{inactive}";
        }
    }
}

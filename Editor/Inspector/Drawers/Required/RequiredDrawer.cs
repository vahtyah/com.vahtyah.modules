using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace VahTyah
{
    public class RequiredDrawer
    {
        private const float ICON_WIDTH = 25f;

        public void DrawProperty(Rect rect, SerializedProperty property, RequiredAttribute attribute, FieldInfo fieldInfo)
        {
            bool isNull = IsNullOrEmpty(property);

            if (isNull)
            {
                Rect fieldRect = new Rect(rect.x, rect.y, rect.width - ICON_WIDTH - 2, rect.height);
                Rect iconRect = new Rect(rect.xMax - ICON_WIDTH, rect.y, ICON_WIDTH, EditorGUIUtility.singleLineHeight);

                EditorGUI.PropertyField(fieldRect, property, new GUIContent(property.displayName), true);
                DrawIcon(iconRect, property, attribute);
            }
            else
            {
                EditorGUI.PropertyField(rect, property, new GUIContent(property.displayName), true);
            }
        }

        public void DrawIcon(Rect iconRect, SerializedProperty property, RequiredAttribute attribute)
        {
            bool isNull = IsNullOrEmpty(property);
            if (!isNull) return;

            string iconName = attribute.IsError ? "console.erroricon.sml" : "console.warnicon.sml";
            string tooltip = GetTooltip(property, attribute);

            GUIContent iconContent = EditorGUIUtility.IconContent(iconName);
            iconContent.tooltip = tooltip;

            // Center the icon vertically and horizontally
            GUIStyle centeredStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            };
            GUI.Label(iconRect, iconContent, centeredStyle);
        }

        public bool ShouldShowIcon(SerializedProperty property)
        {
            return IsNullOrEmpty(property);
        }

        private bool IsNullOrEmpty(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue == null;

                case SerializedPropertyType.String:
                    return string.IsNullOrEmpty(property.stringValue);

                case SerializedPropertyType.ExposedReference:
                    return property.exposedReferenceValue == null;

                default:
                    return false;
            }
        }

        private string GetTooltip(SerializedProperty property, RequiredAttribute attribute)
        {
            if (!string.IsNullOrEmpty(attribute.Message))
            {
                return attribute.Message;
            }

            string prefix = attribute.IsError ? "Error" : "Warning";
            return $"{prefix}: '{property.displayName}' is required!";
        }
    }
}

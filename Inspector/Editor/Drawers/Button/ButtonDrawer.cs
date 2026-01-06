using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace VahTyah
{
    public class ButtonDrawer
    {
        private readonly EditorCustomStyle.ButtonStyles buttonStyles = EditorStyles.GetStyle().buttonStyles;

        public void DrawButton(MethodInfo method, ButtonAttribute attribute, Object[] targets)
        {
            string buttonLabel = string.IsNullOrEmpty(attribute.Label)
                ? ObjectNames.NicifyVariableName(method.Name)
                : attribute.Label;

            Rect buttonRect = GUILayoutUtility.GetRect(0, buttonStyles.buttonHeight, GUILayout.ExpandWidth(true));

            LayerConfiguration config = buttonStyles.normalConfig;
            bool isHovered = buttonRect.Contains(Event.current.mousePosition);
            bool isPressed = isHovered && Event.current.type == EventType.MouseDown && Event.current.button == 0;

            if (isPressed)
            {
                config = buttonStyles.activeConfig;
            }
            else if (isHovered)
            {
                config = buttonStyles.hoverConfig;
            }

            if (Event.current.type == EventType.Repaint)
            {
                LayerDrawingSystem.DrawLayers(buttonRect, config);
            }

            Rect labelRect = new Rect(
                buttonRect.x + buttonStyles.buttonPadding.left,
                buttonRect.y,
                buttonRect.width - buttonStyles.buttonPadding.left - buttonStyles.buttonPadding.right,
                buttonRect.height
            );

            GUI.Label(labelRect, buttonLabel, buttonStyles.labelStyle);

            if (isPressed)
            {
                Event.current.Use();
                foreach (var targetObject in targets)
                {
                    method.Invoke(targetObject, null);
                }
                GUI.changed = true;
            }
        }

        public void DrawSpacing()
        {
            EditorGUILayout.Space(buttonStyles.buttonSpacing);
        }
    }
}


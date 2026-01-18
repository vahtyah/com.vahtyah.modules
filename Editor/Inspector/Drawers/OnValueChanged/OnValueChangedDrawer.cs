using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VahTyah
{
    public class OnValueChangedDrawer
    {
        private const float BUTTON_WIDTH = 25f;

        public void DrawProperty(Rect rect, SerializedProperty property, OnValueChangedAttribute[] attributes, MethodInfo[] methods, Object[] targets)
        {
            Rect fieldRect = new Rect(rect.x, rect.y, rect.width - BUTTON_WIDTH - 2, rect.height);
            Rect buttonRect = new Rect(rect.xMax - BUTTON_WIDTH, rect.y, BUTTON_WIDTH, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(fieldRect, property, new GUIContent(property.displayName), true);
            DrawButton(buttonRect, property, attributes, methods, targets);
        }

        public void DrawButton(Rect buttonRect, SerializedProperty property, OnValueChangedAttribute[] attributes, MethodInfo[] methods, Object[] targets)
        {
            GUIContent buttonContent = EditorGUIUtility.IconContent("PlayButton");
            buttonContent.tooltip = GetTooltip(attributes);

            GUIStyle centeredButton = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(0, 0, 0, 0)
            };

            if (GUI.Button(buttonRect, buttonContent, centeredButton))
            {
                InvokeCallbacks(attributes, methods, property, targets);
            }
        }

        private void InvokeCallbacks(OnValueChangedAttribute[] attributes, MethodInfo[] methods, SerializedProperty property, Object[] targets)
        {
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];

                if (method == null)
                {
                    Debug.LogWarning($"[OnValueChanged] Method '{attributes[i].MethodName}' not found");
                    continue;
                }

                foreach (var target in targets)
                {
                    try
                    {
                        ParameterInfo[] parameters = method.GetParameters();

                        if (parameters.Length == 0)
                        {
                            method.Invoke(target, null);
                        }
                        else if (parameters.Length == 1)
                        {
                            // Get current value from property
                            object value = GetPropertyValue(property, target);
                            method.Invoke(target, new[] { value });
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[OnValueChanged] Error invoking '{method.Name}': {e.Message}");
                    }
                }
            }
        }

        private object GetPropertyValue(SerializedProperty property, Object target)
        {
            var targetType = target.GetType();
            
            // Search through type hierarchy including base classes
            while (targetType != null)
            {
                var field = targetType.GetField(property.name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                if (field != null)
                {
                    return field.GetValue(target);
                }

                targetType = targetType.BaseType;
            }

            return null;
        }

        private string GetTooltip(OnValueChangedAttribute[] attributes)
        {
            if (attributes.Length == 1)
            {
                return $"Invoke: {attributes[0].MethodName}()";
            }
            
            var names = new string[attributes.Length];
            for (int i = 0; i < attributes.Length; i++)
            {
                names[i] = attributes[i].MethodName;
            }
            return $"Invoke: {string.Join(", ", names)}";
        }
    }
}

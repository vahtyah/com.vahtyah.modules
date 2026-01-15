using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VahTyah
{
    public class ButtonDrawer
    {
        private readonly InspectorStyleData.ButtonStyles buttonStyles = InspectorStyle.GetStyle().buttonStyles;
        private readonly Dictionary<string, MethodCache> methodCaches = new();
        private readonly Dictionary<Type, object> defaultValueCache = new();
        private readonly Dictionary<string, ReorderableList> reorderableLists = new();
        private readonly FieldTypeHandler fieldTypeHandler = new();

        private const float LABEL_WIDTH = 100f;
        private const float LIST_HEADER_HEIGHT = 21f;
        private const float LIST_FOOTER_HEIGHT = 20f;
        private const float LABEL_FIELD_SPACING = 2f;

        private static readonly GUILayoutOption[] expandWidthOption = { GUILayout.ExpandWidth(true) };

        #region Data Structures

        private class MethodCache
        {
            public string MethodKey;
            public ParameterInfo[] Parameters;
            public Dictionary<string, object> ParameterValues;
            public object[] ParameterArray;
            public bool HasParameters;
        }

        #endregion

        #region Public Methods

        public void DrawButton(MethodInfo method, ButtonAttribute attribute, Object[] targets)
        {
            string methodKey = GetMethodKey(method, targets[0]);
            var cache = GetOrCreateMethodCache(method, methodKey);
            string buttonLabel = GetButtonLabel(attribute, method);

            if (cache.HasParameters)
            {
                DrawButtonWithParameters(cache, buttonLabel, method, targets, methodKey);
            }
            else
            {
                DrawSimpleButton(cache, buttonLabel, method, targets);
            }
        }

        public void DrawSpacing() => EditorGUILayout.Space(buttonStyles.buttonSpacing);

        public void ClearCache()
        {
            methodCaches.Clear();
            defaultValueCache.Clear();
            reorderableLists.Clear();
        }

        #endregion

        #region Button Drawing

        private void DrawButtonWithParameters(MethodCache cache, string buttonLabel, MethodInfo method,
            Object[] targets, string methodKey)
        {
            float totalHeight = CalculateTotalHeight(cache, methodKey);
            Rect backgroundRect = GUILayoutUtility.GetRect(0, totalHeight, expandWidthOption[0]);

            if (Event.current.type == EventType.Repaint)
            {
                LayerDrawingSystem.DrawLayers(backgroundRect, buttonStyles.backgroundConfig);
            }

            DrawParametersInRect(backgroundRect, cache, buttonLabel, method, targets, methodKey);
        }

        private void DrawSimpleButton(MethodCache cache, string buttonLabel, MethodInfo method, Object[] targets)
        {
            Rect buttonRect = GUILayoutUtility.GetRect(0, buttonStyles.buttonHeight, expandWidthOption[0]);
            DrawStyledButton(buttonRect, cache, buttonLabel, method, targets);
        }

        private void DrawParametersInRect(Rect backgroundRect, MethodCache cache, string buttonLabel, MethodInfo method,
            Object[] targets, string methodKey)
        {
            var layout = new VerticalLayout(backgroundRect, new RectOffset(
                (int)buttonStyles.buttonPadding.left,
                (int)buttonStyles.buttonPadding.right,
                (int)buttonStyles.buttonPadding.top,
                (int)buttonStyles.buttonPadding.bottom + (int)buttonStyles.buttonHeight
            ));

            foreach (var param in cache.Parameters)
            {
                float paramHeight = GetParameterHeight(param.ParameterType, cache.ParameterValues[param.Name],
                    methodKey, param.Name);
                Rect paramRect = layout.GetNextRect(paramHeight);

                int paramIndex = Array.IndexOf(cache.Parameters, param);
                cache.ParameterArray[paramIndex] =
                    DrawParameterField(paramRect, param, cache.ParameterValues, methodKey);
            }

            Rect buttonRect = new Rect(
                backgroundRect.x,
                backgroundRect.yMax - buttonStyles.buttonHeight,
                backgroundRect.width,
                buttonStyles.buttonHeight
            );

            DrawStyledButton(buttonRect, cache, buttonLabel, method, targets);
        }

        private void DrawStyledButton(Rect buttonRect, MethodCache cache, string buttonLabel, MethodInfo method,
            Object[] targets)
        {
            Event currentEvent = Event.current;
            bool isHovered = buttonRect.Contains(currentEvent.mousePosition);
            bool isPressed = isHovered && currentEvent.type == EventType.MouseDown && currentEvent.button == 0;

            LayerConfiguration config = isPressed ? buttonStyles.activeConfig
                : isHovered ? buttonStyles.hoverConfig
                : buttonStyles.normalConfig;

            if (currentEvent.type == EventType.Repaint)
            {
                LayerDrawingSystem.DrawLayers(buttonRect, config);
                GUI.Label(buttonRect, buttonLabel, buttonStyles.labelStyle);
            }

            if (isPressed)
            {
                currentEvent.Use();
                InvokeMethod(method, targets, cache);
                GUI.changed = true;
            }
        }

        #endregion

        #region Parameter Drawing

        private object DrawParameterField(Rect rect, ParameterInfo param, Dictionary<string, object> parameterValues,
            string methodKey)
        {
            Type paramType = param.ParameterType;
            string paramName = param.Name;
            object value = parameterValues[paramName];

            if (IsListType(paramType))
            {
                return DrawListField(rect, paramName, paramType, value, methodKey);
            }

            var (labelRect, fieldRect) = SplitRectForLabelAndField(rect);
            EditorGUI.LabelField(labelRect, ObjectNames.NicifyVariableName(paramName));

            object newValue = fieldTypeHandler.DrawField(fieldRect, paramType, value);
            parameterValues[paramName] = newValue;

            return newValue;
        }

        private object DrawListField(Rect rect, string paramName, Type listType, object value, string methodKey)
        {
            string listKey = $"{methodKey}_{paramName}";
            var reorderableList = GetOrCreateReorderableList(listKey, paramName, listType, value);

            reorderableList.list = value as IList;
            reorderableList.DoList(rect);

            return reorderableList.list;
        }

        private ReorderableList GetOrCreateReorderableList(string listKey, string paramName, Type listType,
            object value)
        {
            if (reorderableLists.TryGetValue(listKey, out ReorderableList existing))
                return existing;

            IList list = value as IList ?? (IList)Activator.CreateInstance(listType);
            Type elementType = listType.GetGenericArguments()[0];

            var reorderableList = new ReorderableList(list, elementType, true, true, true, true);

            reorderableList.drawHeaderCallback = rect =>
                EditorGUI.LabelField(rect, ObjectNames.NicifyVariableName(paramName));

            reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                if (index < reorderableList.list.Count)
                {
                    object element = reorderableList.list[index];
                    reorderableList.list[index] = fieldTypeHandler.DrawField(rect, elementType, element);
                }
            };

            reorderableList.onAddCallback = listRef =>
                listRef.list.Add(GetDefaultValue(elementType));

            reorderableList.elementHeight = fieldTypeHandler.GetHeight(elementType);

            reorderableLists[listKey] = reorderableList;
            return reorderableList;
        }

        #endregion

        #region Height Calculation

        private float CalculateTotalHeight(MethodCache cache, string methodKey)
        {
            float totalHeight = buttonStyles.buttonPadding.top + buttonStyles.buttonPadding.bottom +
                                buttonStyles.buttonHeight;

            foreach (var param in cache.Parameters)
            {
                totalHeight += GetParameterHeight(param.ParameterType, cache.ParameterValues[param.Name], methodKey,
                    param.Name);
                totalHeight += EditorGUIUtility.standardVerticalSpacing;
            }

            return totalHeight;
        }

        private float GetParameterHeight(Type paramType, object value, string methodKey, string paramName)
        {
            if (IsListType(paramType))
            {
                return GetListHeight(paramType, methodKey, paramName);
            }

            return fieldTypeHandler.GetHeight(paramType);
        }

        private float GetListHeight(Type listType, string methodKey, string paramName)
        {
            string listKey = $"{methodKey}_{paramName}";

            if (reorderableLists.TryGetValue(listKey, out ReorderableList list))
            {
                return list.GetHeight();
            }

            Type elementType = listType.GetGenericArguments()[0];
            float elementHeight = fieldTypeHandler.GetHeight(elementType);
            return LIST_HEADER_HEIGHT + elementHeight + LIST_FOOTER_HEIGHT;
        }

        #endregion

        #region Cache Management

        private MethodCache GetOrCreateMethodCache(MethodInfo method, string methodKey)
        {
            if (methodCaches.TryGetValue(methodKey, out MethodCache cache))
                return cache;

            cache = CreateMethodCache(method, methodKey);
            methodCaches[methodKey] = cache;
            return cache;
        }

        private MethodCache CreateMethodCache(MethodInfo method, string methodKey)
        {
            ParameterInfo[] parameters = method.GetParameters();
            var cache = new MethodCache
            {
                MethodKey = methodKey,
                Parameters = parameters,
                HasParameters = parameters.Length > 0,
                ParameterValues = new Dictionary<string, object>(parameters.Length),
                ParameterArray = new object[parameters.Length]
            };

            foreach (var param in parameters)
            {
                cache.ParameterValues[param.Name] = GetDefaultValue(param.ParameterType);
            }

            return cache;
        }

        private object GetDefaultValue(Type type)
        {
            if (IsListType(type))
                return Activator.CreateInstance(type);

            if (defaultValueCache.TryGetValue(type, out object cached))
                return cached;

            object defaultValue = fieldTypeHandler.GetDefaultValue(type);
            defaultValueCache[type] = defaultValue;
            return defaultValue;
        }

        #endregion

        #region Helper Methods

        private string GetMethodKey(MethodInfo method, Object target)
            => $"{target.GetType().Name}.{method.Name}";

        private string GetButtonLabel(ButtonAttribute attribute, MethodInfo method)
            => string.IsNullOrEmpty(attribute.Label) ? ObjectNames.NicifyVariableName(method.Name) : attribute.Label;

        private bool IsListType(Type type)
            => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);

        private (Rect labelRect, Rect fieldRect) SplitRectForLabelAndField(Rect rect)
        {
            Rect labelRect = new Rect(rect.x, rect.y, LABEL_WIDTH, rect.height);
            Rect fieldRect = new Rect(rect.x + LABEL_WIDTH + LABEL_FIELD_SPACING, rect.y,
                rect.width - LABEL_WIDTH - LABEL_FIELD_SPACING, rect.height);
            return (labelRect, fieldRect);
        }

        private void InvokeMethod(MethodInfo method, Object[] targets, MethodCache cache)
        {
            object[] parameters = cache.HasParameters ? cache.ParameterArray : null;
            foreach (var target in targets)
            {
                method.Invoke(target, parameters);
            }
        }

        #endregion

        #region Nested Helper Classes

        private class VerticalLayout
        {
            private float currentY;
            private readonly float contentX;
            private readonly float contentWidth;

            public VerticalLayout(Rect backgroundRect, RectOffset padding)
            {
                currentY = backgroundRect.y + padding.top;
                contentX = backgroundRect.x + padding.left;
                contentWidth = backgroundRect.width - padding.left - padding.right;
            }

            public Rect GetNextRect(float height)
            {
                Rect rect = new Rect(contentX, currentY, contentWidth, height);
                currentY += height + EditorGUIUtility.standardVerticalSpacing;
                return rect;
            }
        }

        #endregion
    }

    #region Field Type Handler

    public class FieldTypeHandler
    {
        public object DrawField(Rect rect, Type type, object value)
        {
            if (type.IsEnum)
                return DrawEnum(rect, type, value);

            if (typeof(Object).IsAssignableFrom(type))
                return EditorGUI.ObjectField(rect, (Object)value, type, true);

            return Type.GetTypeCode(type) switch
            {
                TypeCode.Int32 => EditorGUI.IntField(rect, (int)value),
                TypeCode.Single => EditorGUI.FloatField(rect, (float)value),
                TypeCode.Double => EditorGUI.DoubleField(rect, (double)value),
                TypeCode.String => EditorGUI.TextField(rect, (string)value),
                TypeCode.Boolean => EditorGUI.Toggle(rect, (bool)value),
                _ => DrawComplexType(rect, type, value)
            };
        }

        public float GetHeight(Type type)
        {
            if (type.IsEnum)
                return EditorGUIUtility.singleLineHeight;

            return Type.GetTypeCode(type) switch
            {
                TypeCode.Int32 or TypeCode.Single or TypeCode.Double or TypeCode.String or TypeCode.Boolean
                    => EditorGUIUtility.singleLineHeight,
                _ => GetComplexTypeHeight(type)
            };
        }

        public object GetDefaultValue(Type type)
        {
            if (type.IsEnum)
                return Enum.GetValues(type).GetValue(0);

            return Type.GetTypeCode(type) switch
            {
                TypeCode.String => "",
                TypeCode.Int32 => 0,
                TypeCode.Single => 0f,
                TypeCode.Double => 0.0,
                TypeCode.Boolean => false,
                _ => GetComplexDefaultValue(type)
            };
        }

        private object DrawEnum(Rect rect, Type type, object value)
        {
            if (value == null || !type.IsInstanceOfType(value))
                value = Enum.GetValues(type).GetValue(0);
            return EditorGUI.EnumPopup(rect, (Enum)value);
        }

        private object DrawComplexType(Rect rect, Type type, object value) => type switch
        {
            Type t when t == typeof(Vector2) => EditorGUI.Vector2Field(rect, GUIContent.none, (Vector2)value),
            Type t when t == typeof(Vector3) => EditorGUI.Vector3Field(rect, GUIContent.none, (Vector3)value),
            Type t when t == typeof(Vector4) => EditorGUI.Vector4Field(rect, GUIContent.none, (Vector4)value),
            Type t when t == typeof(Vector2Int) => EditorGUI.Vector2IntField(rect, GUIContent.none, (Vector2Int)value),
            Type t when t == typeof(Vector3Int) => EditorGUI.Vector3IntField(rect, GUIContent.none, (Vector3Int)value),
            Type t when t == typeof(Color) => EditorGUI.ColorField(rect, (Color)value),
            _ => DrawUnsupported(rect, type, value)
        };

        private float GetComplexTypeHeight(Type type)
        {
            if (type == typeof(Vector2))
                return EditorGUI.GetPropertyHeight(SerializedPropertyType.Vector2, GUIContent.none);
            if (type == typeof(Vector3))
                return EditorGUI.GetPropertyHeight(SerializedPropertyType.Vector3, GUIContent.none);
            if (type == typeof(Vector4))
                return EditorGUI.GetPropertyHeight(SerializedPropertyType.Vector4, GUIContent.none);
            if (type == typeof(Vector2Int))
                return EditorGUI.GetPropertyHeight(SerializedPropertyType.Vector2Int, GUIContent.none);
            if (type == typeof(Vector3Int))
                return EditorGUI.GetPropertyHeight(SerializedPropertyType.Vector3Int, GUIContent.none);
            if (type == typeof(Color))
                return EditorGUI.GetPropertyHeight(SerializedPropertyType.Color, GUIContent.none);
            if (typeof(Object).IsAssignableFrom(type))
                return EditorGUI.GetPropertyHeight(SerializedPropertyType.ObjectReference, GUIContent.none);

            return EditorGUIUtility.singleLineHeight;
        }

        private object GetComplexDefaultValue(Type type)
        {
            if (type == typeof(Vector2)) return Vector2.zero;
            if (type == typeof(Vector3)) return Vector3.zero;
            if (type == typeof(Vector4)) return Vector4.zero;
            if (type == typeof(Vector2Int)) return Vector2Int.zero;
            if (type == typeof(Vector3Int)) return Vector3Int.zero;
            if (type == typeof(Color)) return Color.white;
            if (type.IsValueType) return Activator.CreateInstance(type);

            return null;
        }

        private object DrawUnsupported(Rect rect, Type type, object value)
        {
            EditorGUI.LabelField(rect, $"Unsupported:  {type.Name}");
            return value;
        }
    }

    #endregion
}
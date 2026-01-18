using System;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace VahTyah
{
    public class OnValueChangedHandler
    {
        private readonly Object[] targets;
        private readonly SerializedObject serializedObject;
        
        // Store field info and cached previous values
        private readonly struct TrackedField
        {
            public readonly string PropertyPath;
            public readonly FieldInfo FieldInfo;
            public readonly OnValueChangedAttribute[] Attributes;
            public readonly MethodInfo[] Methods;

            public TrackedField(string propertyPath, FieldInfo fieldInfo, OnValueChangedAttribute[] attributes, MethodInfo[] methods)
            {
                PropertyPath = propertyPath;
                FieldInfo = fieldInfo;
                Attributes = attributes;
                Methods = methods;
            }
        }

        private TrackedField[] trackedFields;
        // previousValues[fieldIndex][targetIndex]
        private object[][] previousValues;

        public OnValueChangedHandler(Object[] targets, SerializedObject serializedObject)
        {
            this.targets = targets;
            this.serializedObject = serializedObject;
        }

        public MethodInfo[] RegisterField(string propertyPath, FieldInfo fieldInfo, OnValueChangedAttribute[] attributes, Type targetType)
        {
            // Find methods for each attribute
            MethodInfo[] methods = new MethodInfo[attributes.Length];
            
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            
            for (int i = 0; i < attributes.Length; i++)
            {
                methods[i] = targetType.GetMethod(attributes[i].MethodName, flags);
                
                if (methods[i] == null)
                {
                    // Try to find in base types
                    Type currentType = targetType.BaseType;
                    while (currentType != null && methods[i] == null)
                    {
                        methods[i] = currentType.GetMethod(attributes[i].MethodName, flags);
                        currentType = currentType.BaseType;
                    }
                }
            }

            // Add to tracked fields
            var newField = new TrackedField(propertyPath, fieldInfo, attributes, methods);
            
            // Get initial values for all targets
            object[] initialValues = new object[targets.Length];
            for (int t = 0; t < targets.Length; t++)
            {
                initialValues[t] = GetCurrentValue(fieldInfo, targets[t]);
            }
            
            if (trackedFields == null)
            {
                trackedFields = new[] { newField };
                previousValues = new[] { initialValues };
            }
            else
            {
                Array.Resize(ref trackedFields, trackedFields.Length + 1);
                Array.Resize(ref previousValues, previousValues.Length + 1);
                trackedFields[^1] = newField;
                previousValues[^1] = initialValues;
            }

            return methods;
        }

        public bool HasTrackedFields => trackedFields != null && trackedFields.Length > 0;

        public void CaptureCurrentValues()
        {
            if (trackedFields == null) return;

            for (int i = 0; i < trackedFields.Length; i++)
            {
                for (int t = 0; t < targets.Length; t++)
                {
                    previousValues[i][t] = GetCurrentValue(trackedFields[i].FieldInfo, targets[t]);
                }
            }
        }

        public void CheckAndInvokeCallbacks()
        {
            if (trackedFields == null) return;

            for (int i = 0; i < trackedFields.Length; i++)
            {
                for (int t = 0; t < targets.Length; t++)
                {
                    object currentValue = GetCurrentValue(trackedFields[i].FieldInfo, targets[t]);
                    object previousValue = previousValues[i][t];

                    if (!ValuesEqual(currentValue, previousValue))
                    {
                        // Value changed - invoke callbacks on this target
                        InvokeCallbacks(trackedFields[i], currentValue, targets[t]);
                        
                        // Update stored value
                        previousValues[i][t] = currentValue;
                    }
                }
            }
        }

        private object GetCurrentValue(FieldInfo fieldInfo, Object target)
        {
            try
            {
                object value = fieldInfo.GetValue(target);
                
                // Clone value types and strings to avoid reference issues
                if (value == null) return null;
                if (fieldInfo.FieldType.IsValueType) return value;
                if (value is string) return value;
                
                // For reference types, store a reference (comparison will be by reference)
                return value;
            }
            catch
            {
                return null;
            }
        }

        private bool ValuesEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            
            // For Unity Objects, compare using Unity's null check
            if (a is Object unityObjA && b is Object unityObjB)
            {
                return unityObjA == unityObjB;
            }
            
            return a.Equals(b);
        }

        private void InvokeCallbacks(TrackedField field, object newValue, Object target)
        {
            for (int i = 0; i < field.Methods.Length; i++)
            {
                MethodInfo method = field.Methods[i];
                
                if (method == null)
                {
                    UnityEngine.Debug.LogWarning($"[OnValueChanged] Method '{field.Attributes[i].MethodName}' not found on {target.GetType().Name}");
                    continue;
                }

                try
                {
                    ParameterInfo[] parameters = method.GetParameters();

                    if (parameters.Length == 0)
                    {
                        // Parameterless method
                        method.Invoke(target, null);
                    }
                    else if (parameters.Length == 1 && parameters[0].ParameterType.IsAssignableFrom(field.FieldInfo.FieldType))
                    {
                        // Method accepts the new value
                        method.Invoke(target, new[] { newValue });
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"[OnValueChanged] Method '{method.Name}' has incompatible parameters. Expected parameterless or ({field.FieldInfo.FieldType.Name})");
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"[OnValueChanged] Error invoking '{method.Name}': {e.Message}");
                }
            }
        }
    }
}

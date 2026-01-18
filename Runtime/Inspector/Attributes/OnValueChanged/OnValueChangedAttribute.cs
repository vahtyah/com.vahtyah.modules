using System;

namespace VahTyah
{
    /// <summary>
    /// Invokes a method when the field value changes in the Inspector
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class OnValueChangedAttribute : Attribute
    {
        public string MethodName { get; }

        /// <summary>
        /// Call a method when this field's value changes
        /// </summary>
        /// <param name="methodName">Name of the method to invoke. Method can be parameterless or accept the new value.</param>
        public OnValueChangedAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}

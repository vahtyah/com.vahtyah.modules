using System;

namespace VahTyah
{
    /// <summary>
    /// Attribute to display a button in the inspector that calls a method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ButtonAttribute : Attribute
    {
        public string Label { get; }
        public int Order { get; }

        /// <summary>
        /// Creates a button attribute
        /// </summary>
        /// <param name="label">Button label (uses method name if empty)</param>
        /// <param name="order">Display order (lower values appear first)</param>
        public ButtonAttribute(string label = "", int order = 0)
        {
            Label = label;
            Order = order;
        }
    }
}

using System;

namespace VahTyah
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public abstract class GroupAttribute : Attribute
    {
        public string ID { get; }
        public string Label { get; }
        public int Order { get; }

        protected GroupAttribute(string id, string label = "", int order = 0)
        {
            ID = id;
            Label = string.IsNullOrEmpty(label) ? id : label;
            Order = order;
        }
    }
}
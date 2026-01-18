using System;

namespace VahTyah
{
    /// <summary>
    /// Automatically get/find component references
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class AutoRefAttribute : Attribute
    {
        public RefSource Source { get; }
        public bool IncludeInactive { get; }

        /// <summary>
        /// Auto get/find reference
        /// </summary>
        /// <param name="source">Where to search (default: Self)</param>
        /// <param name="includeInactive">Include inactive GameObjects (default: false)</param>
        public AutoRefAttribute(RefSource source = RefSource.Self, bool includeInactive = false)
        {
            Source = source;
            IncludeInactive = includeInactive;
        }
    }

    public enum RefSource
    {
        /// <summary>GetComponent from same GameObject</summary>
        Self,
        
        /// <summary>GetComponentInChildren</summary>
        Children,
        
        /// <summary>GetComponentInParent</summary>
        Parent,
        
        /// <summary>FindObjectOfType in scene</summary>
        Scene
    }
}

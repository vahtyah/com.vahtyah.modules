using System;

namespace VahTyah
{
    /// <summary>
    /// Automatically find and assign asset references from the project
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class AssetRefAttribute : Attribute
    {
        public string AssetName { get; }
        public string AssetPath { get; }

        /// <summary>
        /// Find first asset matching field type
        /// </summary>
        public AssetRefAttribute()
        {
            AssetName = null;
            AssetPath = null;
        }

        /// <summary>
        /// Find asset by name
        /// </summary>
        /// <param name="assetName">Asset name (without extension), or null to find first matching type</param>
        /// <param name="assetPath">Folder path (e.g. "Assets/Data"), or null to search entire project</param>
        public AssetRefAttribute(string assetName = null, string assetPath = null)
        {
            AssetName = assetName;
            AssetPath = assetPath;
        }
    }
}

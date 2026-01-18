using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace VahTyah
{
    /// <summary>
    /// Interface for drawing grouped properties
    /// </summary>
    public interface IGroupDrawer
    {
        void Draw(PropertyGroup propertyGroup);
    }

    /// <summary>
    /// Represents a group of serialized properties with a drawer
    /// </summary>
    public class PropertyGroup
    {
        public GroupAttribute Attribute { get; }
        public IGroupDrawer Drawer { get; }
        public List<SerializedProperty> Properties { get; }
        public Dictionary<string, (FieldInfo field, AutoRefAttribute attr)> AutoRefAttributes { get; }
        public Dictionary<string, (FieldInfo field, AssetRefAttribute attr)> AssetRefAttributes { get; }
        public Dictionary<string, (FieldInfo field, OnValueChangedAttribute[] attrs, MethodInfo[] methods)> OnValueChangedAttributes { get; }
        public AutoRefDrawer AutoRefDrawer { get; private set; }
        public AssetRefDrawer AssetRefDrawer { get; private set; }
        public OnValueChangedDrawer OnValueChangedDrawer { get; private set; }
        public Object Target { get; private set; }
        public Object[] Targets { get; private set; }

        public PropertyGroup(GroupAttribute attribute, IGroupDrawer drawer)
        {
            Attribute = attribute;
            Drawer = drawer;
            Properties = new List<SerializedProperty>();
            AutoRefAttributes = new Dictionary<string, (FieldInfo, AutoRefAttribute)>();
            AssetRefAttributes = new Dictionary<string, (FieldInfo, AssetRefAttribute)>();
            OnValueChangedAttributes = new Dictionary<string, (FieldInfo, OnValueChangedAttribute[], MethodInfo[])>();
        }

        public void SetAutoRefDrawer(AutoRefDrawer drawer, Object target)
        {
            AutoRefDrawer = drawer;
            Target = target;
        }

        public void SetAssetRefDrawer(AssetRefDrawer drawer)
        {
            AssetRefDrawer = drawer;
        }

        public void SetOnValueChangedDrawer(OnValueChangedDrawer drawer, Object[] targets)
        {
            OnValueChangedDrawer = drawer;
            Targets = targets;
        }

        public void AddAutoRefAttribute(string propertyPath, FieldInfo field, AutoRefAttribute attribute)
        {
            AutoRefAttributes[propertyPath] = (field, attribute);
        }

        public void AddAssetRefAttribute(string propertyPath, FieldInfo field, AssetRefAttribute attribute)
        {
            AssetRefAttributes[propertyPath] = (field, attribute);
        }

        public void AddOnValueChangedAttribute(string propertyPath, FieldInfo field, OnValueChangedAttribute[] attrs, MethodInfo[] methods)
        {
            OnValueChangedAttributes[propertyPath] = (field, attrs, methods);
        }

        public bool HasAutoRef(SerializedProperty property)
        {
            return AutoRefAttributes.ContainsKey(property.propertyPath);
        }

        public bool HasAssetRef(SerializedProperty property)
        {
            return AssetRefAttributes.ContainsKey(property.propertyPath);
        }

        public bool HasOnValueChanged(SerializedProperty property)
        {
            return OnValueChangedAttributes.ContainsKey(property.propertyPath);
        }

        public (FieldInfo field, AutoRefAttribute attr)? GetAutoRefAttribute(SerializedProperty property)
        {
            if (AutoRefAttributes.TryGetValue(property.propertyPath, out var info))
                return info;
            return null;
        }

        public (FieldInfo field, AssetRefAttribute attr)? GetAssetRefAttribute(SerializedProperty property)
        {
            if (AssetRefAttributes.TryGetValue(property.propertyPath, out var info))
                return info;
            return null;
        }

        public (FieldInfo field, OnValueChangedAttribute[] attrs, MethodInfo[] methods)? GetOnValueChangedAttribute(SerializedProperty property)
        {
            if (OnValueChangedAttributes.TryGetValue(property.propertyPath, out var info))
                return info;
            return null;
        }

        public void Draw()
        {
            Drawer.Draw(this);
        }
    }

    /// <summary>
    /// Registry that maps GroupAttribute types to their drawers
    /// </summary>
    public static class GroupDrawerRegistry
    {
        private static Dictionary<Type, Func<GroupAttribute, IGroupDrawer>> drawerFactories;

        static GroupDrawerRegistry()
        {
            drawerFactories = new Dictionary<Type, Func<GroupAttribute, IGroupDrawer>>
            {
                // Register BoxGroup drawer
                { typeof(BoxGroupAttribute), attr => new BoxGroupDrawer() },

                // Easy to add more with attribute parameters: 
                // { typeof(FoldoutGroupAttribute), attr =>
                //     {
                //         var foldout = attr as FoldoutGroupAttribute;
                //         return new FoldoutGroupDrawer(foldout.ID, foldout. DefaultExpanded);
                //     }
                // },
            };
        }

        /// <summary>
        /// Get drawer instance for the given attribute
        /// </summary>
        public static IGroupDrawer GetDrawer(GroupAttribute attribute)
        {
            Type attrType = attribute.GetType();

            if (drawerFactories.TryGetValue(attrType, out var factory))
            {
                return factory(attribute);
            }

            // Fallback to BoxGroup drawer if not found
            return new BoxGroupDrawer();
        }

        /// <summary>
        /// Register a custom drawer at runtime (for extensions)
        /// </summary>
        public static void RegisterDrawer<TAttribute>(Func<GroupAttribute, IGroupDrawer> factory)
            where TAttribute : GroupAttribute
        {
            drawerFactories[typeof(TAttribute)] = factory;
        }
    }
}

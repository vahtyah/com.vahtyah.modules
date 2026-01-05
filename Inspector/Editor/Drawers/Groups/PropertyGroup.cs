using System;
using System.Collections.Generic;
using UnityEditor;

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

        public PropertyGroup(GroupAttribute attribute, IGroupDrawer drawer)
        {
            Attribute = attribute;
            Drawer = drawer;
            Properties = new List<SerializedProperty>();
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
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VahTyah
{
    [FilePath("VahTyah/PlayModeSaveSystem.asset", FilePathAttribute.Location.PreferencesFolder)]
    public class PlayModeSaveSystem : ScriptableSingleton<PlayModeSaveSystem>
    {
        [SerializeField] private List<ComponentData> savedComponentDatas = new();

        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode) return;
            
            // Delay to ensure scene is fully loaded
            EditorApplication.delayCall += ApplySavedData;
        }

        private static void ApplySavedData()
        {
            if (instance.savedComponentDatas.Count == 0) return;

            foreach (var data in instance.savedComponentDatas.ToList())
            {
                Component component = null;

                // Try to find by GlobalObjectId first
                if (GlobalObjectId.TryParse(data.globalId, out var goid))
                {
                    component = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(goid) as Component;
                }

                // For prefabs, try unpacked ID
                if (component == null && !string.IsNullOrEmpty(data.globalIdUnpacked))
                {
                    if (GlobalObjectId.TryParse(data.globalIdUnpacked, out var goidUnpacked))
                    {
                        component = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(goidUnpacked) as Component;
                    }
                }

                // Fallback: try by InstanceID (works if same session)
                if (component == null)
                {
                    component = EditorUtility.InstanceIDToObject(data.componentInstanceId) as Component;
                }

                if (component != null)
                {
                    ApplyComponentData(data, component);
                }
            }

            instance.savedComponentDatas.Clear();
            instance.Save(true);
        }

        public static void SaveComponent(Component component)
        {
            if (component == null) return;

            var globalId = GetGlobalId(component);
            var existingData = instance.savedComponentDatas.FirstOrDefault(r => r.globalId == globalId);
            
            if (existingData != null)
            {
                instance.savedComponentDatas.Remove(existingData);
            }
            else
            {
                var data = GetComponentData(component);
                instance.savedComponentDatas.Add(data);
            }

            instance.Save(true);
        }

        public static bool IsSaved(Component component)
        {
            if (component == null) return false;
            var globalId = GetGlobalId(component);
            return instance.savedComponentDatas.Any(r => r.globalId == globalId);
        }

        public static void ClearSavedData(Component component)
        {
            if (component == null) return;
            var globalId = GetGlobalId(component);
            instance.savedComponentDatas.RemoveAll(r => r.globalId == globalId);
            instance.Save(true);
        }

        public static void ClearAllSavedData()
        {
            instance.savedComponentDatas.Clear();
            instance.Save(true);
        }

        private static string GetGlobalId(Object obj)
        {
            return GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
        }

        private static string GetGlobalIdUnpacked(Object obj)
        {
            var goid = GlobalObjectId.GetGlobalObjectIdSlow(obj);
            
            // Unpack for prefab instances (they have different IDs in play mode)
            var unpackedFileId = (goid.targetObjectId ^ goid.targetPrefabId) & 0x7fffffffffffffff;
            return $"GlobalObjectId_V1-{goid.identifierType}-{goid.assetGUID}-{unpackedFileId}-0";
        }

        private static ComponentData GetComponentData(Component component)
        {
            var data = new ComponentData
            {
                globalId = GetGlobalId(component),
                globalIdUnpacked = GetGlobalIdUnpacked(component),
                componentInstanceId = component.GetInstanceID(),
                componentTypeName = component.GetType().AssemblyQualifiedName
            };

            var serializedObject = new SerializedObject(component);
            var property = serializedObject.GetIterator();

            if (!property.Next(true)) return data;

            do
            {
                if (property.name == "m_Script") continue;
                if (!property.editable) continue;

                try
                {
                    var path = property.propertyPath;

                    switch (property.propertyType)
                    {
                        case SerializedPropertyType.Integer:
                            data.propertyPaths.Add(path);
                            data.propertyTypes.Add((int)property.propertyType);
                            data.stringValues.Add(property.intValue.ToString());
                            break;
                        case SerializedPropertyType.Boolean:
                            data.propertyPaths.Add(path);
                            data.propertyTypes.Add((int)property.propertyType);
                            data.stringValues.Add(property.boolValue.ToString());
                            break;
                        case SerializedPropertyType.Float:
                            data.propertyPaths.Add(path);
                            data.propertyTypes.Add((int)property.propertyType);
                            data.stringValues.Add(property.floatValue.ToString("R"));
                            break;
                        case SerializedPropertyType.String:
                            data.propertyPaths.Add(path);
                            data.propertyTypes.Add((int)property.propertyType);
                            data.stringValues.Add(property.stringValue ?? "");
                            break;
                        case SerializedPropertyType.Enum:
                            data.propertyPaths.Add(path);
                            data.propertyTypes.Add((int)property.propertyType);
                            data.stringValues.Add(property.enumValueIndex.ToString());
                            break;
                        case SerializedPropertyType.Vector2:
                            data.propertyPaths.Add(path);
                            data.propertyTypes.Add((int)property.propertyType);
                            data.stringValues.Add(JsonUtility.ToJson(property.vector2Value));
                            break;
                        case SerializedPropertyType.Vector3:
                            data.propertyPaths.Add(path);
                            data.propertyTypes.Add((int)property.propertyType);
                            data.stringValues.Add(JsonUtility.ToJson(property.vector3Value));
                            break;
                        case SerializedPropertyType.Vector4:
                            data.propertyPaths.Add(path);
                            data.propertyTypes.Add((int)property.propertyType);
                            data.stringValues.Add(JsonUtility.ToJson(property.vector4Value));
                            break;
                        case SerializedPropertyType.Quaternion:
                            data.propertyPaths.Add(path);
                            data.propertyTypes.Add((int)property.propertyType);
                            data.stringValues.Add(JsonUtility.ToJson(property.quaternionValue));
                            break;
                        case SerializedPropertyType.Color:
                            data.propertyPaths.Add(path);
                            data.propertyTypes.Add((int)property.propertyType);
                            data.stringValues.Add(JsonUtility.ToJson(property.colorValue));
                            break;
                        case SerializedPropertyType.Rect:
                            data.propertyPaths.Add(path);
                            data.propertyTypes.Add((int)property.propertyType);
                            data.stringValues.Add(JsonUtility.ToJson(property.rectValue));
                            break;
                        case SerializedPropertyType.Bounds:
                            data.propertyPaths.Add(path);
                            data.propertyTypes.Add((int)property.propertyType);
                            data.stringValues.Add(JsonUtility.ToJson(property.boundsValue));
                            break;
                        case SerializedPropertyType.Vector2Int:
                            data.propertyPaths.Add(path);
                            data.propertyTypes.Add((int)property.propertyType);
                            data.stringValues.Add(JsonUtility.ToJson(property.vector2IntValue));
                            break;
                        case SerializedPropertyType.Vector3Int:
                            data.propertyPaths.Add(path);
                            data.propertyTypes.Add((int)property.propertyType);
                            data.stringValues.Add(JsonUtility.ToJson(property.vector3IntValue));
                            break;
                        case SerializedPropertyType.ObjectReference:
                            if (property.objectReferenceValue != null)
                            {
                                var assetPath = AssetDatabase.GetAssetPath(property.objectReferenceValue);
                                if (!string.IsNullOrEmpty(assetPath))
                                {
                                    // It's an asset - store by path
                                    data.objectRefPaths.Add(path);
                                    data.objectRefAssetPaths.Add(assetPath);
                                    data.objectRefAssetTypes.Add(property.objectReferenceValue.GetType().AssemblyQualifiedName);
                                }
                                else
                                {
                                    // It's a scene object - store by GlobalObjectId
                                    data.objectRefPaths.Add(path);
                                    data.objectRefAssetPaths.Add(GetGlobalId(property.objectReferenceValue));
                                    data.objectRefAssetTypes.Add(""); // Empty means it's a GlobalId, not asset path
                                }
                            }
                            break;
                    }
                }
                catch
                {
                    // Skip properties that can't be serialized
                }
            }
            while (property.NextVisible(true));

            return data;
        }

        private static void ApplyComponentData(ComponentData data, Component targetComponent)
        {
            if (targetComponent == null) return;

            Undo.RecordObject(targetComponent, "PlayMode Save Load");

            var serializedObject = new SerializedObject(targetComponent);
            serializedObject.Update();

            // Apply regular values
            for (int i = 0; i < data.propertyPaths.Count; i++)
            {
                try
                {
                    var property = serializedObject.FindProperty(data.propertyPaths[i]);
                    if (property == null) continue;

                    var propType = (SerializedPropertyType)data.propertyTypes[i];
                    var value = data.stringValues[i];

                    switch (propType)
                    {
                        case SerializedPropertyType.Integer:
                            property.intValue = int.Parse(value);
                            break;
                        case SerializedPropertyType.Boolean:
                            property.boolValue = bool.Parse(value);
                            break;
                        case SerializedPropertyType.Float:
                            property.floatValue = float.Parse(value);
                            break;
                        case SerializedPropertyType.String:
                            property.stringValue = value;
                            break;
                        case SerializedPropertyType.Enum:
                            property.enumValueIndex = int.Parse(value);
                            break;
                        case SerializedPropertyType.Vector2:
                            property.vector2Value = JsonUtility.FromJson<Vector2>(value);
                            break;
                        case SerializedPropertyType.Vector3:
                            property.vector3Value = JsonUtility.FromJson<Vector3>(value);
                            break;
                        case SerializedPropertyType.Vector4:
                            property.vector4Value = JsonUtility.FromJson<Vector4>(value);
                            break;
                        case SerializedPropertyType.Quaternion:
                            property.quaternionValue = JsonUtility.FromJson<Quaternion>(value);
                            break;
                        case SerializedPropertyType.Color:
                            property.colorValue = JsonUtility.FromJson<Color>(value);
                            break;
                        case SerializedPropertyType.Rect:
                            property.rectValue = JsonUtility.FromJson<Rect>(value);
                            break;
                        case SerializedPropertyType.Bounds:
                            property.boundsValue = JsonUtility.FromJson<Bounds>(value);
                            break;
                        case SerializedPropertyType.Vector2Int:
                            property.vector2IntValue = JsonUtility.FromJson<Vector2Int>(value);
                            break;
                        case SerializedPropertyType.Vector3Int:
                            property.vector3IntValue = JsonUtility.FromJson<Vector3Int>(value);
                            break;
                    }
                }
                catch
                {
                    // Skip
                }
            }

            // Apply object references
            for (int i = 0; i < data.objectRefPaths.Count; i++)
            {
                try
                {
                    var property = serializedObject.FindProperty(data.objectRefPaths[i]);
                    if (property == null) continue;

                    var pathOrGlobalId = data.objectRefAssetPaths[i];
                    var assetType = data.objectRefAssetTypes[i];
                    
                    Object obj = null;
                    
                    if (!string.IsNullOrEmpty(assetType))
                    {
                        // It's an asset path
                        var type = Type.GetType(assetType);
                        if (type != null)
                        {
                            obj = AssetDatabase.LoadAssetAtPath(pathOrGlobalId, type);
                        }
                    }
                    else
                    {
                        // It's a GlobalObjectId
                        if (GlobalObjectId.TryParse(pathOrGlobalId, out var goid))
                        {
                            obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(goid);
                        }
                    }

                    property.objectReferenceValue = obj;
                }
                catch
                {
                    // Skip
                }
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(targetComponent);
        }

        [Serializable]
        public class ComponentData
        {
            // Component identification using GlobalObjectId (survives Play/Edit mode)
            public string globalId;
            public string globalIdUnpacked; // For prefab instances
            public int componentInstanceId; // Fallback
            public string componentTypeName;

            // Regular properties
            public List<string> propertyPaths = new();
            public List<int> propertyTypes = new();
            public List<string> stringValues = new();

            // Object references
            public List<string> objectRefPaths = new();
            public List<string> objectRefAssetPaths = new(); // Asset path or GlobalId string
            public List<string> objectRefAssetTypes = new(); // Empty if GlobalId
        }
    }
}
#endif

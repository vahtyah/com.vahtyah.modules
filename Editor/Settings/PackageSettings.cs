#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Compilation;

namespace VahTyah
{
    public static class PackageSettings
    {
        public const string DEFINE_SYMBOL = "VAHTYAH_CUSTOM_INSPECTOR";
        private const string EDITOR_STYLE_PATH = "Assets/Editor/EditorStyle";
        private const string ASSET_NAME = "EditorStyleDatabase.asset";

        [MenuItem("Tools/VahTyah/Custom Editor Style", false, 1)]
        public static void CreateEditorStyleDatabase()
        {
            if (!Directory.Exists(EDITOR_STYLE_PATH))
            {
                Directory.CreateDirectory(EDITOR_STYLE_PATH);
                AssetDatabase.Refresh();
            }

            var existingAsset = EditorUtils.GetAsset<EditorStyleDatabase>();

            if (existingAsset != null)
            {
                Selection.activeObject = existingAsset;
                EditorGUIUtility.PingObject(existingAsset);
                return;
            }

            string assetPath = Path.Combine(EDITOR_STYLE_PATH, ASSET_NAME);
            var database =  EditorUtils.CreateAsset<EditorStyleDatabase>(assetPath, true);
            database.AddDefaultStyle();
            EditorStyles.SetStyleDatabase(database);

            Selection.activeObject = database;
            EditorGUIUtility.PingObject(database);
        }

        private const string MENU_PATH = "Tools/VahTyah/Disable";

        [MenuItem(MENU_PATH, false, 100)]
        public static void ToggleCustomDrawers()
        {
            if (IsDefineSymbolEnabled(DEFINE_SYMBOL))
            {
                RemoveDefineSymbol(DEFINE_SYMBOL);
            }
            else
            {
                AddDefineSymbol(DEFINE_SYMBOL);
            }
            CompilationPipeline.RequestScriptCompilation();
        }

        [MenuItem(MENU_PATH, true)]
        public static bool ValidateToggleCustomDrawers()
        {
            Menu.SetChecked(MENU_PATH, !IsDefineSymbolEnabled(DEFINE_SYMBOL));
            return true;
        }

        // Helper methods
        public static void AddDefineSymbol(string symbol)
        {
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            if (!defines.Contains(symbol))
            {
                if (string.IsNullOrEmpty(defines))
                {
                    defines = symbol;
                }
                else
                {
                    defines += ";" + symbol;
                }

                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
            }
        }

        private static void RemoveDefineSymbol(string symbol)
        {
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            if (defines.Contains(symbol))
            {
                var symbolList = defines.Split(';');
                var newDefines = string.Join(";", System.Array.FindAll(symbolList, s => s != symbol));
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefines);
            }
        }

        private static bool IsDefineSymbolEnabled(string symbol)
        {
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            return defines.Contains(symbol);
        }
    }
}
#endif
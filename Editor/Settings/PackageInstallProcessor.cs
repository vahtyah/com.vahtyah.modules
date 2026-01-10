#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace VahTyah
{
    [InitializeOnLoad]
    public static class PackageInstallProcessor
    {
        private const string PREFS_KEY = "VahTyah_PackageInstalled";

        static PackageInstallProcessor()
        {
            if (!EditorPrefs.GetBool(PREFS_KEY, false))
            {
                PackageSettings.AddDefineSymbol(PackageSettings.DEFINE_SYMBOL);
                EditorPrefs.SetBool(PREFS_KEY, true);
            }
        }

        private static void AddDefineSymbol(string symbol)
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
    }
}
#endif
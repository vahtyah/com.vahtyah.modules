using UnityEditor;

namespace VahTyah
{
    [InitializeOnLoad]
    public static class EditorStyles
    {
        private static EditorStyleDatabase styleDatabase;
        public static EditorCustomStyle Style;

        static EditorStyles()
        {
            styleDatabase = EditorUtils.GetAsset<EditorStyleDatabase>();
            Style = styleDatabase.GetStyle();
        }

        public static void EnsureStyleDatabaseExists()
        {
            if (styleDatabase == null)
            {
                styleDatabase = EditorUtils.GetAsset<EditorStyleDatabase>();
            }

            Style ??= styleDatabase.GetStyle();
        }
    }
}
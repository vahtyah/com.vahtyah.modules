using UnityEditor;

namespace VahTyah
{
    [InitializeOnLoad]
    public static class EditorStyles
    {
        private static EditorStyleDatabase styleDatabase;
        private static EditorCustomStyle Style;

        static EditorStyles()
        {
            EnsureStyleDatabaseExists();
        }

        public static void EnsureStyleDatabaseExists()
        {
            if(Style != null) return;
            
            if (styleDatabase == null)
            {
                styleDatabase = EditorUtils.GetAsset<EditorStyleDatabase>();
            }
            
            Style = styleDatabase?.GetStyle() ?? EditorStyleDatabase.GetDefaultStyle();
        }

        public static EditorCustomStyle GetStyle()
        {
            EnsureStyleDatabaseExists();
            return Style;
        }
    }
}
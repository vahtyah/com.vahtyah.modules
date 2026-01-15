using UnityEditor;

namespace VahTyah
{
    [InitializeOnLoad]
    public static class InspectorStyle
    {
        private static EditorStyleDatabase styleDatabase;
        private static InspectorStyleData styleData;

        static InspectorStyle()
        {
            EnsureStyleDatabaseExists();
        }

        public static void EnsureStyleDatabaseExists()
        {
            if(styleData != null) return;
            
            if (styleDatabase == null)
            {
                styleDatabase = EditorUtils.GetAsset<EditorStyleDatabase>();
            }
            
            styleData = styleDatabase?.GetStyle() ?? EditorStyleDatabase.GetDefaultStyle();
        }

        public static InspectorStyleData GetStyle()
        {
            EnsureStyleDatabaseExists();
            return styleData;
        }
        
        public static void SetStyleDatabase(EditorStyleDatabase database)
        {
            styleDatabase = database;
            styleData = styleDatabase?.GetStyle() ?? EditorStyleDatabase.GetDefaultStyle();
        }
    }
}
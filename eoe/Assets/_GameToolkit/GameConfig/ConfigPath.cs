namespace _KITSystem.Config
{
    public static class ConfigPath
    {
        private const string EditorPrefsKeyFolderPrefix = "ConfigDownloader_Folder";

        private static string folder = "Assets/_Source/Configs";
        
        public static string Folder
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorPrefs.GetString(EditorPrefsKeyFolderPrefix);
#endif
                return folder;
            }
            set
            {
#if UNITY_EDITOR
                UnityEditor.EditorPrefs.SetString(EditorPrefsKeyFolderPrefix, value);
                return;
#endif
                folder = value;
            }
        }
    }
}
namespace _GameToolkit.GameConfig
{
    public static class ConfigPath
    {
        private const string EditorPrefsKeyFolderPrefix = "ConfigDownloader_Folder";

        private static string folder = "Assets/_TDSAssets/Config";
        
        public static string Folder
        {
            get
            {
#if UNITY_EDITOR
                var p = UnityEditor.EditorPrefs.GetString(EditorPrefsKeyFolderPrefix);
                if (!string.IsNullOrEmpty(p)) return p;
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

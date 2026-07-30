// Unity
using UnityEngine;

namespace GUPS.AntiCheat.Editor.Helper
{
    /// <summary>
    /// Reusable GUI styles for editor windows.
    /// </summary>
    public static class StyleHelper
    {
        /// <summary>
        /// Gets a style with a translucent dark background (1x1 tinted texture).
        /// </summary>
        public static GUIStyle DarkBackground
        {
            get
            {
                GUIStyle var_GUIStyle = new GUIStyle();
                var_GUIStyle.normal.background = TextureHelper.MakeTexture(1, 1, new Color(0.1f, 0.1f, 0.1f, 0.25f));
                return var_GUIStyle;
            }
        }
    }
}

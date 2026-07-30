// Unity
using UnityEngine;

namespace GUPS.AntiCheat.Editor.Helper
{
    /// <summary>
    /// Helpers for procedurally creating editor textures.
    /// </summary>
    public static class TextureHelper
    {
        /// <summary>
        /// Creates a solid-color <see cref="Texture2D"/> of the given size.
        /// </summary>
        /// <param name="_Width">Texture width in pixels.</param>
        /// <param name="_Height">Texture height in pixels.</param>
        /// <param name="_Color">Fill color.</param>
        /// <returns>The created texture.</returns>
        public static Texture2D MakeTexture(int _Width, int _Height, Color _Color)
        {
            Color[] pix = new Color[_Width * _Height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = _Color;

            Texture2D result = new Texture2D(_Width, _Height, TextureFormat.ARGB32, false);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }
    }
}

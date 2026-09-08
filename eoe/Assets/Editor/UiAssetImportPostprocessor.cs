using UnityEditor;
using UnityEngine;

namespace _TDS.Editor
{
    public sealed class UiAssetImportPostprocessor : AssetPostprocessor
    {
        private const string UiArtPath = "Assets/Resources/UI/Art/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(UiArtPath, System.StringComparison.OrdinalIgnoreCase)) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        }
    }
}

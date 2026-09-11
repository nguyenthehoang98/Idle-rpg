using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class TextureResize4
{
    private const int Multiple = 4;

    [MenuItem("Assets/Texture Resize 4/Up", false, 2000)]
    private static void ResizeUp()
    {
        ResizeSelectedTexture(true);
    }

    [MenuItem("Assets/Texture Resize 4/Down", false, 2010)]
    private static void ResizeDown()
    {
        ResizeSelectedTexture(false);
    }

    private static void ResizeSelectedTexture(bool roundUp)
    {
        Texture2D source = Selection.activeObject as Texture2D;
        string assetPath = source == null ? string.Empty : AssetDatabase.GetAssetPath(source);
        if (source == null || string.IsNullOrEmpty(assetPath)) return;

        int targetWidth = MultipleOfFour(source.width, roundUp);
        int targetHeight = MultipleOfFour(source.height, roundUp);
        if (targetWidth == source.width && targetHeight == source.height)
        {
            EditorUtility.DisplayDialog("Texture Resize 4", $"Already {source.width} x {source.height}.", "OK");
            return;
        }

        string extension = Path.GetExtension(assetPath).ToLowerInvariant();
        if (!IsSupported(extension))
        {
            EditorUtility.DisplayDialog(
                "Texture Resize 4",
                "Supported formats: PNG, JPG/JPEG and TGA.",
                "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog(
                "Texture Resize 4",
                $"Resize {source.width} x {source.height} to {targetWidth} x {targetHeight}?\n\nThe source file will be overwritten.",
                "Resize",
                "Cancel")) return;

        Texture2D resized = null;
        try
        {
            resized = Resize(source, targetWidth, targetHeight);
            byte[] encoded = Encode(resized, extension);
            string fullPath = Path.GetFullPath(assetPath);
            string tempPath = fullPath + ".resize4.tmp";

            File.WriteAllBytes(tempPath, encoded);
            File.Replace(tempPath, fullPath, null);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            EditorUtility.DisplayDialog("Texture Resize 4", $"Resized to {targetWidth} x {targetHeight}.", "OK");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorUtility.DisplayDialog("Texture Resize 4", exception.Message, "OK");
        }
        finally
        {
            if (resized != null) UnityEngine.Object.DestroyImmediate(resized);
        }
    }

    [MenuItem("Assets/Texture Resize 4/Up", true)]
    private static bool ValidateResizeUp()
    {
        return CanResizeSelectedTexture();
    }

    [MenuItem("Assets/Texture Resize 4/Down", true)]
    private static bool ValidateResizeDown()
    {
        return CanResizeSelectedTexture();
    }

    private static bool CanResizeSelectedTexture()
    {
        Texture2D texture = Selection.activeObject as Texture2D;
        return texture != null && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(texture));
    }

    internal static int MultipleOfFour(int value, bool roundUp)
    {
        int quotient = value / Multiple;
        if (roundUp && value % Multiple != 0) quotient++;
        return Mathf.Max(Multiple, quotient * Multiple);
    }

    private static Texture2D Resize(Texture2D source, int width, int height)
    {
        RenderTexture previous = RenderTexture.active;
        RenderTexture target = RenderTexture.GetTemporary(
            width,
            height,
            0,
            RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Default);

        try
        {
            Graphics.Blit(source, target);
            RenderTexture.active = target;

            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply(false, false);
            return result;
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
        }
    }

    private static bool IsSupported(string extension)
    {
        return extension == ".png" || extension == ".jpg" || extension == ".jpeg" || extension == ".tga";
    }

    private static byte[] Encode(Texture2D texture, string extension)
    {
        switch (extension)
        {
            case ".jpg":
            case ".jpeg":
                return ImageConversion.EncodeToJPG(texture, 95);
            case ".tga":
                return ImageConversion.EncodeToTGA(texture);
            default:
                return ImageConversion.EncodeToPNG(texture);
        }
    }
}

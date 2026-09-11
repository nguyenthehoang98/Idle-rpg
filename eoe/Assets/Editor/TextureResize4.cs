using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class TextureResize4
{
    private const int Multiple = 4;

    [MenuItem("Assets/Texture Resize 4: Nearest", false, 2000)]
    private static void ResizeNearest()
    {
        Texture2D source = Selection.activeObject as Texture2D;
        if (source == null) return;

        ResizeSelectedTexture(
            NearestMultipleOfFour(source.width),
            NearestMultipleOfFour(source.height));
    }

    [MenuItem("Assets/Texture Resize 4: User Input", false, 2010)]
    private static void ResizeWithUserInput()
    {
        Texture2D source = Selection.activeObject as Texture2D;
        if (source == null) return;

        TextureResize4InputWindow.Show(source.width, source.height, (width, height) =>
        {
            ResizeSelectedTexture(width, height);
        });
    }

    private static void ResizeSelectedTexture(int targetWidth, int targetHeight)
    {
        Texture2D source = Selection.activeObject as Texture2D;
        string assetPath = source == null ? string.Empty : AssetDatabase.GetAssetPath(source);
        if (source == null || string.IsNullOrEmpty(assetPath)) return;
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

    [MenuItem("Assets/Texture Resize 4: Nearest", true)]
    [MenuItem("Assets/Texture Resize 4: User Input", true)]
    private static bool CanResizeSelectedTexture()
    {
        Texture2D texture = Selection.activeObject as Texture2D;
        return texture != null && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(texture));
    }

    internal static int NearestMultipleOfFour(int value)
    {
        return Mathf.Max(Multiple, Mathf.RoundToInt(value / (float)Multiple) * Multiple);
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

internal sealed class TextureResize4InputWindow : EditorWindow
{
    string dimensions;
    string error;
    Action<int, int> onConfirm;

    public static void Show(int width, int height, Action<int, int> confirmed)
    {
        var window = CreateInstance<TextureResize4InputWindow>();
        window.titleContent = new GUIContent("Texture Resize 4");
        window.minSize = new Vector2(320f, 120f);
        window.maxSize = new Vector2(320f, 120f);
        window.dimensions = $"{width}:{height}";
        window.onConfirm = confirmed;
        window.ShowModalUtility();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Enter size as width:height", EditorStyles.wordWrappedLabel);
        dimensions = EditorGUILayout.TextField("Size", dimensions);

        if (!string.IsNullOrEmpty(error))
            EditorGUILayout.HelpBox(error, MessageType.Error);

        GUILayout.FlexibleSpace();
        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Resize"))
            {
                if (TryParse(dimensions, out int width, out int height))
                {
                    var callback = onConfirm;
                    Close();
                    callback(width, height);
                }
                else
                {
                    error = "Use two positive integers, for example 512:256.";
                }
            }

            if (GUILayout.Button("Cancel"))
                Close();
        }
    }

    static bool TryParse(string value, out int width, out int height)
    {
        width = 0;
        height = 0;
        string[] parts = value.Split(':');
        return parts.Length == 2
            && int.TryParse(parts[0].Trim(), out width)
            && int.TryParse(parts[1].Trim(), out height)
            && width > 0
            && height > 0;
    }
}

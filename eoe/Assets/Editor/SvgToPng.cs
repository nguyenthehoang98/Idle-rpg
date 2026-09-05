using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SvgToPng
{
    private const int OutputSize = 256;
    private const float PixelsPerUnit = 256f;
    private const int RenderLayer = 31;

    private static readonly string[] HeroIds = { "101", "102", "103", "104", "105" };
    private static readonly string[] MonsterIds = { "1001", "1002", "1003" };

    [MenuItem("Tools/Assets/SVG to PNG/Convert battle SVGs")]
    public static void ConvertBattleSvgs()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        var outputs = new Dictionary<string, Sprite>();

        foreach (string id in HeroIds)
        {
            ConvertOne(
                $"Assets/_TDSAssets/Battles/Heros/Svg/hero.{id}.svg",
                $"Assets/_TDSAssets/Battles/Heros/Png/hero.{id}.png",
                projectRoot,
                outputs);
        }

        foreach (string id in MonsterIds)
        {
            ConvertOne(
                $"Assets/_TDSAssets/Battles/Monsters/Svg/monster.{id}.svg",
                $"Assets/_TDSAssets/Battles/Monsters/Png/monster.{id}.png",
                projectRoot,
                outputs);
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        ConfigurePrefabs(outputs);
        AssetDatabase.SaveAssets();
        Debug.Log($"[SvgToPng] Converted and assigned {outputs.Count} battle sprites.");
    }

    private static void ConvertOne(
        string svgPath,
        string pngPath,
        string projectRoot,
        Dictionary<string, Sprite> outputs)
    {
        AssetDatabase.ImportAsset(svgPath, ImportAssetOptions.ForceUpdate);
        Sprite source = AssetDatabase.LoadAssetAtPath<Sprite>(svgPath);
        if (source == null)
        {
            throw new InvalidOperationException($"SVG did not import as a Sprite: {svgPath}");
        }

        string absolutePath = Path.Combine(projectRoot, pngPath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
        File.WriteAllBytes(absolutePath, Render(source));
        AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = AssetImporter.GetAtPath(pngPath) as TextureImporter;
        if (importer == null)
        {
            throw new InvalidOperationException($"PNG did not import as a Texture2D: {pngPath}");
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = PixelsPerUnit;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.filterMode = FilterMode.Bilinear;
        importer.SaveAndReimport();

        Sprite result = AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
        if (result == null)
        {
            throw new InvalidOperationException($"PNG did not import as a Sprite: {pngPath}");
        }

        outputs[pngPath] = result;
    }

    private static byte[] Render(Sprite sprite)
    {
        GameObject spriteObject = new GameObject("SvgToPng.Sprite");
        GameObject cameraObject = new GameObject("SvgToPng.Camera");
        RenderTexture renderTexture = new RenderTexture(OutputSize, OutputSize, 0, RenderTextureFormat.ARGB32);
        Texture2D texture = new Texture2D(OutputSize, OutputSize, TextureFormat.RGBA32, false);
        RenderTexture previous = RenderTexture.active;

        try
        {
            spriteObject.layer = RenderLayer;
            SpriteRenderer renderer = spriteObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = Color.white;

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.cullingMask = 1 << RenderLayer;
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.clear;
            camera.targetTexture = renderTexture;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.transform.LookAt(Vector3.zero);

            Bounds bounds = renderer.bounds;
            float aspect = renderTexture.width / (float)renderTexture.height;
            camera.orthographicSize = Mathf.Max(bounds.extents.y, bounds.extents.x / aspect) * 1.05f;
            camera.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10f);

            RenderTexture.active = renderTexture;
            camera.Render();
            texture.ReadPixels(new Rect(0, 0, OutputSize, OutputSize), 0, 0);
            texture.Apply();
            RenderTexture.active = previous;
            return texture.EncodeToPNG();
        }
        finally
        {
            RenderTexture.active = previous;
            Camera camera = cameraObject.GetComponent<Camera>();
            if (camera != null) camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(texture);
            UnityEngine.Object.DestroyImmediate(renderTexture);
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(spriteObject);
        }
    }

    private static void ConfigurePrefabs(Dictionary<string, Sprite> sprites)
    {
        foreach (string id in HeroIds)
        {
            ConfigurePrefab(
                $"Assets/_TDSAssets/Battles/Heros/hero.{id}.prefab",
                sprites[$"Assets/_TDSAssets/Battles/Heros/Png/hero.{id}.png"]);
        }

        foreach (string id in MonsterIds)
        {
            ConfigurePrefab(
                $"Assets/_TDSAssets/Battles/Monsters/monster.{id}.prefab",
                sprites[$"Assets/_TDSAssets/Battles/Monsters/Png/monster.{id}.png"]);
        }
    }

    private static void ConfigurePrefab(string prefabPath, Sprite sprite)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            foreach (SpriteRenderer renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                renderer.sprite = sprite;
            }

            foreach (MonoBehaviour animation in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                SerializedObject serialized = new SerializedObject(animation);
                SerializedProperty sprites = serialized.FindProperty("sprites");
                if (sprites == null || !sprites.isArray) continue;

                for (int i = 0; i < sprites.arraySize; i++)
                {
                    sprites.GetArrayElementAtIndex(i).objectReferenceValue = sprite;
                }

                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}

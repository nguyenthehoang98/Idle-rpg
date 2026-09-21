using System;
using System.IO;
using _TDS.Battle;
using _TDS.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ConfigureGameplayUi
{
    private const string BuildScene = "Assets/Scenes/GamePlayScene.unity";
    private const string LegacyScene = "Assets/Scenes/GameplayScene.unity";
    private const string UpgradeTemplateFolder = "Assets/_TDSAssets/Gameplay/Prefabs";
    private const string UpgradeTemplatePath = UpgradeTemplateFolder + "/WaveUpgradePanel.prefab";

    [MenuItem("TDS/Create Wave Upgrade UI Template")]
    public static void CreateWaveUpgradeTemplate()
    {
        GameObject prefab = EnsureWaveUpgradeTemplate();
        if (prefab != null) Selection.activeObject = prefab;
    }

    [MenuItem("TDS/Configure Gameplay UI")]
    public static void Configure()
    {
        ConfigureScene(BuildScene);
        if (File.Exists(LegacyScene)) ConfigureScene(LegacyScene);
        AssetDatabase.SaveAssets();
        Debug.Log("[Gameplay UI] Inspector references configured.");
    }

    private static void ConfigureScene(string scenePath)
    {
        if (!File.Exists(scenePath)) return;

        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        GameObject root = GameObject.Find("Root");
        if (root == null)
        {
            Debug.LogError($"[Gameplay UI] Root not found in {scenePath}");
            return;
        }

        GameObject health = FindInScene(scene, "health", "Health");
        GameObject energy = FindInScene(scene, "energy", "Energy");
        ConfigureResource(health, "Health");
        ConfigureResource(energy, "Energy");

        GameplayUiCommon common = root.GetComponent<GameplayUiCommon>();
        if (common == null) common = Undo.AddComponent<GameplayUiCommon>(root);

        GameplayUiControl control = root.GetComponent<GameplayUiControl>();
        if (control == null) control = Undo.AddComponent<GameplayUiControl>(root);

        CircuitTickRunner circuitRunner = UnityEngine.Object.FindFirstObjectByType<CircuitTickRunner>(FindObjectsInactive.Include);
        SetReference(common, "health", health);
        SetReference(common, "energy", energy);
        SetReference(control, "uiCommon", common);
        SetReference(control, "circuitRunner", circuitRunner);

        WaveUpgradePanel upgradePanel = root.GetComponentInChildren<WaveUpgradePanel>(true);
        if (upgradePanel == null)
        {
            GameObject template = EnsureWaveUpgradeTemplate();
            if (template != null)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(template, root.transform) as GameObject;
                if (instance != null)
                {
                    Undo.RegisterCreatedObjectUndo(instance, "Add Wave Upgrade UI");
                    upgradePanel = instance.GetComponent<WaveUpgradePanel>();
                }
            }
        }

        if (upgradePanel == null)
        {
            upgradePanel = Undo.AddComponent<WaveUpgradePanel>(root);
            upgradePanel.ConfigureInspectorUi();
        }

        GameplayScene gameplayScene = root.GetComponent<GameplayScene>();
        if (gameplayScene != null) SetReference(gameplayScene, "upgradePanel", upgradePanel);
        else Debug.LogError($"[Gameplay UI] GameplayScene component not found in {scenePath}");

        EditorUtility.SetDirty(upgradePanel);
        EditorUtility.SetDirty(root);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static GameObject EnsureWaveUpgradeTemplate()
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(UpgradeTemplatePath);
        if (existing != null) return existing;

        EnsureFolder("Assets/_TDSAssets", "Gameplay");
        EnsureFolder("Assets/_TDSAssets/Gameplay", "Prefabs");

        GameObject templateRoot = new GameObject("WaveUpgradePanel");
        WaveUpgradePanel panel = templateRoot.AddComponent<WaveUpgradePanel>();
        panel.ConfigureInspectorUi();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(templateRoot, UpgradeTemplatePath);
        UnityEngine.Object.DestroyImmediate(templateRoot);
        AssetDatabase.SaveAssets();
        return prefab;
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
    }

    private static void ConfigureResource(GameObject resource, string resourceName)
    {
        if (resource == null)
        {
            Debug.LogError($"[Gameplay UI] {resourceName} object not found");
            return;
        }

        resource.name = resourceName;
        Transform fill = FindDirectChild(resource.transform, "fill", "Image");
        Transform text = FindDirectChild(resource.transform, "text", "Text (TMP)", "Text");

        if (fill == null) Debug.LogError($"[Gameplay UI] fill not found under {resourceName}");
        else
        {
            fill.name = "fill";
            Image image = fill.GetComponent<Image>();
            if (image != null) image.type = Image.Type.Filled;
        }

        if (text == null) Debug.LogError($"[Gameplay UI] text not found under {resourceName}");
        else text.name = "text";
    }

    private static GameObject FindInScene(Scene scene, params string[] names)
    {
        foreach (GameObject sceneRoot in scene.GetRootGameObjects())
        {
            foreach (Transform child in sceneRoot.GetComponentsInChildren<Transform>(true))
            {
                foreach (string name in names)
                {
                    if (string.Equals(child.name, name, StringComparison.OrdinalIgnoreCase)) return child.gameObject;
                }
            }
        }

        return null;
    }

    private static Transform FindDirectChild(Transform root, params string[] names)
    {
        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            foreach (string name in names)
            {
                if (string.Equals(child.name, name, StringComparison.OrdinalIgnoreCase)) return child;
            }
        }

        return null;
    }

    private static void SetReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null) throw new InvalidOperationException($"Property '{propertyName}' not found on {target.name}");
        property.objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}

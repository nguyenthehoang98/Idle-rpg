using _TDS.Core;
using _TDS.GameplayScene;
using _TDS.GameplayScene.UI;
using _TDS.GameplayScene.Unit;
using _Toolkit.Collider;
using _Toolkit.SkillSystem.Core;
using _Toolkit.Updater;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace _TDS.Editor
{
    public static class GameplaySceneSetup
    {
        [MenuItem("Tools/Setup/Setup Gameplay Scene")]
        public static void SetupScene()
        {
            if (!EditorUtility.DisplayDialog("Setup Gameplay Scene",
                "This will add missing GameObjects to the current scene.\nContinue?",
                "Yes", "Cancel"))
                return;

            SetupUpdaterOwner();
            SetupGameManager();
            SetupBaseCore();
            SetupWaveManager();
            SetupGameplayUI();
            ActivateInactiveHero();

            EditorUtility.DisplayDialog("Done",
                "Gameplay scene setup complete!\n\nAdded:\n- SkillTickRunner\n- ProjectileTickRunner\n- GameManager\n- BaseCore\n- WaveManager\n- GameplayUI\n- Activated hero (1)",
                "OK");
        }

        private static void SetupUpdaterOwner()
        {
            GameObject updaterOwner = GameObject.Find("UpdaterOwner");
            if (updaterOwner == null)
            {
                Debug.LogError("UpdaterOwner not found in scene");
                return;
            }

            UpdateRunner runner = updaterOwner.GetComponent<UpdateRunner>();
            if (runner == null)
            {
                Debug.LogError("UpdateRunner not found on UpdaterOwner");
                return;
            }

            Transform parent = updaterOwner.transform;

            // Add SkillTickRunner
            GameObject skillGO = CreateChild(parent, "SkillTickRunner");
            SkillTickRunner skillRunner = skillGO.AddComponent<SkillTickRunner>();

            // Add ProjectileTickRunner
            GameObject projGO = CreateChild(parent, "ProjectileTickRunner");
            ProjectileTickRunner projRunner = projGO.AddComponent<ProjectileTickRunner>();

            // Register in UpdateRunner
            SerializedObject so = new SerializedObject(runner);
            SerializedProperty updatables = so.FindProperty("updatables");

            // Add SkillTickRunner
            int skillIndex = updatables.arraySize;
            updatables.InsertArrayElementAtIndex(skillIndex);
            updatables.GetArrayElementAtIndex(skillIndex).objectReferenceValue = skillRunner;

            // Add ProjectileTickRunner
            int projIndex = updatables.arraySize;
            updatables.InsertArrayElementAtIndex(projIndex);
            updatables.GetArrayElementAtIndex(projIndex).objectReferenceValue = projRunner;

            so.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log("[Setup] Added SkillTickRunner + ProjectileTickRunner to UpdaterOwner");
        }

        private static void SetupGameManager()
        {
            if (GameObject.FindObjectOfType<GameManager>() != null)
            {
                Debug.Log("[Setup] GameManager already exists, skipping");
                return;
            }

            GameObject go = new GameObject("GameManager");
            go.AddComponent<GameManager>();

            Debug.Log("[Setup] Created GameManager");
        }

        private static void SetupBaseCore()
        {
            if (GameObject.FindObjectOfType<BaseCore>() != null)
            {
                Debug.Log("[Setup] BaseCore already exists, skipping");
                return;
            }

            GameObject go = new GameObject("BaseCore");
            go.transform.position = Vector3.zero;

            BaseCore baseCore = go.AddComponent<BaseCore>();

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetCircleSprite();
            sr.color = new Color(0.2f, 0.6f, 1f, 0.8f);
            sr.sortingOrder = -1;
            go.transform.localScale = Vector3.one * 1.5f;

            Debug.Log("[Setup] Created BaseCore at (0,0)");
        }

        private static void SetupWaveManager()
        {
            if (GameObject.FindObjectOfType<WaveManager>() != null)
            {
                Debug.Log("[Setup] WaveManager already exists, skipping");
                return;
            }

            GameObject go = new GameObject("WaveManager");
            go.AddComponent<WaveManager>();

            Debug.Log("[Setup] Created WaveManager");
        }

        private static void SetupGameplayUI()
        {
            if (GameObject.FindObjectOfType<GameplayUI>() != null)
            {
                Debug.Log("[Setup] GameplayUI already exists, skipping");
                return;
            }

            // Create Canvas
            GameObject canvasGO = new GameObject("GameplayCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // Create HUD Panel
            GameObject hudPanel = CreateUIPanel(canvasGO.transform, "HUD Panel", new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -10), new Vector2(300, 80));

            TextMeshProUGUI txtBaseHp = CreateText(hudPanel.transform, "TxtBaseHp", "Base HP: 100/100", new Vector2(0, 50));
            TextMeshProUGUI txtWave = CreateText(hudPanel.transform, "TxtWave", "Wave: 0/10", new Vector2(0, 25));
            TextMeshProUGUI txtAlive = CreateText(hudPanel.transform, "TxtAlive", "Alive: 0", new Vector2(0, 0));

            // Create GameOver Panel (hidden)
            GameObject gameOverPanel = CreateUIPanel(canvasGO.transform, "GameOver Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400, 200));
            gameOverPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);
            TextMeshProUGUI txtGameOver = CreateText(gameOverPanel.transform, "TxtGameOver", "GAME OVER", new Vector2(0, 20));
            txtGameOver.fontSize = 36;
            txtGameOver.alignment = TextAlignmentOptions.Center;
            gameOverPanel.SetActive(false);

            // Create Victory Panel (hidden)
            GameObject victoryPanel = CreateUIPanel(canvasGO.transform, "Victory Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400, 200));
            victoryPanel.GetComponent<Image>().color = new Color(0, 0.5f, 0, 0.8f);
            TextMeshProUGUI txtVictory = CreateText(victoryPanel.transform, "TxtVictory", "VICTORY!", new Vector2(0, 20));
            txtVictory.fontSize = 36;
            txtVictory.alignment = TextAlignmentOptions.Center;
            victoryPanel.SetActive(false);

            // Add GameplayUI component
            GameplayUI gameplayUI = canvasGO.AddComponent<GameplayUI>();

            // Wire references via SerializedObject
            SerializedObject uiSO = new SerializedObject(gameplayUI);
            uiSO.FindProperty("txtBaseHp").objectReferenceValue = txtBaseHp;
            uiSO.FindProperty("txtWave").objectReferenceValue = txtWave;
            uiSO.FindProperty("txtAliveMonsters").objectReferenceValue = txtAlive;
            uiSO.FindProperty("panelGameOver").objectReferenceValue = gameOverPanel;
            uiSO.FindProperty("panelVictory").objectReferenceValue = victoryPanel;
            uiSO.FindProperty("txtGameOverMessage").objectReferenceValue = txtGameOver;
            uiSO.FindProperty("txtVictoryMessage").objectReferenceValue = txtVictory;
            uiSO.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log("[Setup] Created GameplayUI Canvas with HUD + GameOver + Victory panels");
        }

        private static void ActivateInactiveHero()
        {
            GameObject herosContainer = GameObject.Find("Heros");
            if (herosContainer == null)
            {
                Debug.Log("[Setup] Heros container not found");
                return;
            }

            // Activate the second hero (Isometric Diamond (1))
            foreach (Transform child in herosContainer.transform)
            {
                if (child.name == "Isometric Diamond (1)")
                {
                    child.gameObject.SetActive(true);
                    Debug.Log("[Setup] Activated hero: Isometric Diamond (1)");
                    break;
                }
            }
        }

        private static GameObject CreateChild(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            return go;
        }

        private static GameObject CreateUIPanel(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;

            go.AddComponent<CanvasRenderer>();
            Image img = go.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.5f);

            return go;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, Vector2 position)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(0, 30);

            go.AddComponent<CanvasRenderer>();
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 18;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Left;

            return tmp;
        }

        private static Sprite GetCircleSprite()
        {
            // Create a simple circle sprite procedurally
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2f;
            float radius = size / 2f - 1;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist <= radius)
                    {
                        tex.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64);
        }
    }
}

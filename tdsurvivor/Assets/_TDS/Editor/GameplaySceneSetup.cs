using System;
using System.Collections.Generic;
using _TDS.Config;
using _TDS.Core;
using _TDS.GameplayScene;
using _TDS.GameplayScene.SkillSystem;
using _TDS.GameplayScene.UI;
using _TDS.GameplayScene.Unit;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Object = UnityEngine.Object;

namespace _TDS.Editor
{
    /// <summary>
    /// Menu 1-click để scene GamePlay chạy được ngay:
    /// GameManager + BaseCore + WaveManager + GameplayUI + ForceTargetInput
    /// + đủ weapon theo HeroConfig.json (clone từ weapon mẫu có sẵn trong scene).
    /// Weapon là thực thể tấn công (thay cho Hero) – tự find target + auto attack.
    /// Idempotent - cái nào đã có thì bỏ qua.
    /// </summary>
    public static class GameplaySceneSetup
    {
        private const string HeroConfigJson = "Assets/_TDS assets/Config/HeroConfig.json";

        [Serializable]
        private class HeroConfigJsonWrapper
        {
            public List<WeaponUpgradeData> heroes;
        }

        [MenuItem("Tools/Setup/Setup Gameplay Scene")]
        public static void SetupScene()
        {
            SetupGameManager();
            SetupBaseCore();
            SetupWaveManager();
            SetupForceTargetInput();
            SetupGameplayUI();
            SetupHeroes();
            RegisterProjectilePrefabs.Register();

            Debug.Log("[Setup] Gameplay scene setup done");
        }

        /* ------------------------------------------------------------------ */
        /* Managers                                                            */
        /* ------------------------------------------------------------------ */

        private static void SetupGameManager()
        {
            if (Object.FindObjectOfType<GameManager>() != null) return;
            new GameObject("GameManager").AddComponent<GameManager>();
            Debug.Log("[Setup] Created GameManager");
        }

        private static void SetupBaseCore()
        {
            if (Object.FindObjectOfType<BaseCore>() != null) return;

            GameObject go = new GameObject("BaseCore");
            go.transform.position = Vector3.zero;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetCircleSprite();
            sr.color = new Color(0.2f, 0.6f, 1f, 0.8f);
            sr.sortingOrder = -1;
            go.transform.localScale = Vector3.one * 1.5f;

            go.AddComponent<BaseCore>();
            Debug.Log("[Setup] Created BaseCore at (0,0)");
        }

        private static void SetupWaveManager()
        {
            if (Object.FindObjectOfType<WaveManager>() != null) return;
            new GameObject("WaveManager").AddComponent<WaveManager>();
            Debug.Log("[Setup] Created WaveManager");
        }

        private static void SetupForceTargetInput()
        {
            if (Object.FindObjectOfType<ForceTargetInput>() != null) return;
            new GameObject("ForceTargetInput").AddComponent<ForceTargetInput>();
            Debug.Log("[Setup] Created ForceTargetInput");
        }

        /* ------------------------------------------------------------------ */
        /* UI                                                                  */
        /* ------------------------------------------------------------------ */

        private static void SetupGameplayUI()
        {
            if (Object.FindObjectOfType<GameplayUI>() != null) return;

            GameObject canvasGO = new GameObject("GameplayCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            GameObject hud = CreatePanel(canvasGO.transform, "HUD", new Vector2(0, 1), new Vector2(10, -10), new Vector2(300, 80));
            TextMeshProUGUI txtBaseHp = CreateText(hud.transform, "TxtBaseHp", "Base HP: 100/100", 50);
            TextMeshProUGUI txtWave = CreateText(hud.transform, "TxtWave", "Wave: 0/10", 25);
            TextMeshProUGUI txtAlive = CreateText(hud.transform, "TxtAlive", "Alive: 0", 0);

            GameObject gameOver = CreatePanel(canvasGO.transform, "GameOverPanel", Vector2.one * 0.5f, Vector2.zero, new Vector2(400, 200));
            gameOver.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);
            TextMeshProUGUI txtGameOver = CreateText(gameOver.transform, "TxtGameOver", "GAME OVER", 20);
            txtGameOver.fontSize = 36;
            txtGameOver.alignment = TextAlignmentOptions.Center;
            gameOver.SetActive(false);

            GameObject victory = CreatePanel(canvasGO.transform, "VictoryPanel", Vector2.one * 0.5f, Vector2.zero, new Vector2(400, 200));
            victory.GetComponent<Image>().color = new Color(0, 0.5f, 0, 0.8f);
            TextMeshProUGUI txtVictory = CreateText(victory.transform, "TxtVictory", "VICTORY!", 20);
            txtVictory.fontSize = 36;
            txtVictory.alignment = TextAlignmentOptions.Center;
            victory.SetActive(false);

            GameplayUI ui = canvasGO.AddComponent<GameplayUI>();

            SerializedObject so = new SerializedObject(ui);
            so.FindProperty("txtBaseHp").objectReferenceValue = txtBaseHp;
            so.FindProperty("txtWave").objectReferenceValue = txtWave;
            so.FindProperty("txtAliveMonsters").objectReferenceValue = txtAlive;
            so.FindProperty("panelGameOver").objectReferenceValue = gameOver;
            so.FindProperty("panelVictory").objectReferenceValue = victory;
            so.FindProperty("txtGameOverMessage").objectReferenceValue = txtGameOver;
            so.FindProperty("txtVictoryMessage").objectReferenceValue = txtVictory;
            so.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log("[Setup] Created GameplayUI (HUD + GameOver + Victory)");
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor == Vector2.one * 0.5f ? Vector2.one * 0.5f : new Vector2(0, 1);
            rt.anchoredPosition = position;
            rt.sizeDelta = size;

            go.AddComponent<CanvasRenderer>();
            Image img = go.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.5f);
            return go;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, float y)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(0, y);
            rt.sizeDelta = new Vector2(0, 30);

            go.AddComponent<CanvasRenderer>();
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 18;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Left;
            return tmp;
        }

        /* ------------------------------------------------------------------ */
        /* Heroes - đủ 5 hero theo HeroConfig.json                             */
        /* ------------------------------------------------------------------ */

        private static void SetupHeroes()
        {
            HeroConfigJsonWrapper config = ReadHeroConfig();
            if (config?.heroes == null || config.heroes.Count == 0)
            {
                Debug.LogError($"[Setup] Cannot read hero config at '{HeroConfigJson}'");
                return;
            }

            GameObject container = GameObject.Find("Heros");
            if (container == null) container = new GameObject("Heros");

            // Weapon mẫu: GO có component Weapon trong scene (weapon là thực thể tấn công, không còn Hero)
            Weapon template = null;
            List<Weapon> existing = new List<Weapon>();
            foreach (Transform child in container.transform)
            {
                Weapon w = child.GetComponentInChildren<Weapon>();
                if (w == null) continue;
                existing.Add(w);
                if (template == null) template = w;
            }

            if (template == null)
            {
                Debug.LogError("[Setup] Need at least 1 GO with Weapon under 'Heros' as template");
                return;
            }

            for (int i = 0; i < config.heroes.Count; i++)
            {
                WeaponUpgradeData data = config.heroes[i];

                Weapon weapon;
                if (i < existing.Count)
                {
                    weapon = existing[i];
                }
                else
                {
                    GameObject go = Object.Instantiate(template.gameObject, container.transform);
                    go.name = data.name;
                    weapon = go.GetComponentInChildren<Weapon>();
                    existing.Add(weapon);
                }

                ApplyWeaponConfig(weapon.gameObject, data, i, config.heroes.Count);
            }
        }

        private static void ApplyWeaponConfig(GameObject go, WeaponUpgradeData data, int index, int total)
        {
            Undo.RecordObject(go.transform, "Setup Weapons");
            go.name = data.name;

            // Đặt quanh base thành vòng tròn
            float angle = (360f * index / total) * Mathf.Deg2Rad;
            go.transform.position = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * 2.5f;

            Weapon weapon = go.GetComponentInChildren<Weapon>();
            if (weapon == null)
            {
                Debug.LogWarning($"[Setup] '{data.name}' has no Weapon, skip stats");
                return;
            }

            // Stat của Weapon nạp từ WeaponConfig/SkillConfig theo weaponId lúc runtime,
            // editor chỉ cần gán đúng id.
            SerializedObject wso = new SerializedObject(weapon);
            SetValue(wso, "weaponId", data.weaponId);
            wso.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(go);
            Debug.Log($"[Setup] Weapon '{data.name}' configured");
        }

        private static void SetValue(SerializedObject so, string prop, float value)
        {
            SerializedProperty p = so.FindProperty(prop);
            if (p == null)
            {
                Debug.LogWarning($"[Setup] Weapon has no property '{prop}'");
                return;
            }

            if (p.propertyType == SerializedPropertyType.Float) p.floatValue = value;
            else if (p.propertyType == SerializedPropertyType.Integer) p.intValue = Mathf.RoundToInt(value);
        }

        private static void SetValue(SerializedObject so, string prop, string value)
        {
            SerializedProperty p = so.FindProperty(prop);
            if (p == null)
            {
                Debug.LogWarning($"[Setup] Weapon has no property '{prop}'");
                return;
            }

            if (p.propertyType == SerializedPropertyType.String) p.stringValue = value;
        }

        private static HeroConfigJsonWrapper ReadHeroConfig()
        {
            string path = System.IO.Path.GetFullPath(HeroConfigJson);
            if (!System.IO.File.Exists(path))
            {
                Debug.LogError($"[Setup] Config file not found: {path}");
                return null;
            }

            return JsonUtility.FromJson<HeroConfigJsonWrapper>(System.IO.File.ReadAllText(path));
        }

        /* ------------------------------------------------------------------ */

        private static Sprite GetCircleSprite()
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2f;
            float radius = size / 2f - 1;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    tex.SetPixel(x, y,
                        Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) <= radius
                            ? Color.white
                            : Color.clear);
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, 64);
        }
    }
}

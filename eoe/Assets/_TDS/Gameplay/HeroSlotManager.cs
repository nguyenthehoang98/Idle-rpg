using System;
using System.Collections;
using System.Collections.Generic;
using _GameToolkit.GameConfig;
using _GameToolkit.ResourceManagement;
using _TDS.Battle;
using _TDS.GameConfig;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _TDS.Gameplay
{
    /// <summary>
    /// Quản lý các slot đặt hero và hiển thị highlight tĩnh theo vị trí Energy Roll.
    /// Nhận danh sách heroId từ GameplayScene.heroIds, id thoả config mới tạo.
    /// </summary>
    public class HeroSlotManager : MonoBehaviour
    {
        [SerializeField] private HeroSlot[] slots;
        [SerializeField] private bool enableArrow = true;
        [SerializeField, Min(0.01f)] private float arrowPlayTime = 0.5f;
        [Header("Highlight Cell")]
        [Tooltip("Một cell highlight duy nhất, được đặt tại slot đích sau mỗi Roll.")]
        [SerializeField] private Transform highlightCell;

        private SpriteRenderer[] slotRenderers;
        private Coroutine arrowSequenceCoroutine;
        private static readonly Color NormalColor = Color.white;
        private static readonly Color HighlightColor = ParseColor("FDE047");

        private void Awake()
        {
            if (slots == null || slots.Length == 0)
            {
                Debug.LogError($"[{name}] No hero slots assigned", this);
            }
            
            CacheRenderers();
            if (highlightCell == null) highlightCell = transform.Find("Highlight Cell") ?? transform.Find("highlight");
            SetAllNormal();
            
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null) slots[i].Deactivate();
            }

            SetHighlightCellActive(false);
        }

        private void OnEnable()
        {
            if (!Application.isPlaying) return;

            if (enableArrow && arrowSequenceCoroutine == null)
            {
                arrowSequenceCoroutine = StartCoroutine(PlayArrowSequence());
            }

            // Highlight is static while charging and moves only after a circuit roll.
        }

        private void OnDisable()
        {
            if (arrowSequenceCoroutine != null)
            {
                StopCoroutine(arrowSequenceCoroutine);
                arrowSequenceCoroutine = null;
            }

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null) slots[i].Deactivate();
            }

            SetHighlightCellActive(false);
        }

        private void CacheRenderers()
        {
            if (slots == null) return;
            slotRenderers = new SpriteRenderer[slots.Length];
            for (int i = 0; i < slots.Length; i++)
                slotRenderers[i] = slots[i] != null ? slots[i].GetComponent<SpriteRenderer>() : null;
        }

        public void RefreshHighlight(EnergyCircuit circuit)
        {
            if (circuit == null || slotRenderers == null) return;
            for (int i = 0; i < slotRenderers.Length; i++)
            {
                SpriteRenderer r = slotRenderers[i];
                if (r == null) continue;
                bool isHighlight = false;
                if (i < circuit.SlotCount)
                {
                    isHighlight = i == circuit.HighlightIndex;
                }
                r.color = isHighlight ? HighlightColor : NormalColor;
            }

            if (highlightCell != null && slots != null && circuit.HighlightIndex >= 0 && circuit.HighlightIndex < slots.Length)
            {
                HeroSlot slot = slots[circuit.HighlightIndex];
                if (slot != null)
                {
                    highlightCell.position = slot.transform.position;
                    SetHighlightCellActive(true);
                }
            }
        }

        private IEnumerator PlayArrowSequence()
        {
            if (slots == null || slots.Length == 0) yield break;

            WaitForSeconds playDelay = new WaitForSeconds(Mathf.Max(0.01f, arrowPlayTime));
            while (true)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    if (slots[i] != null) slots[i].Deactivate();
                    yield return playDelay;
                }
            }
        }

        private void SetHighlightCellActive(bool active)
        {
            if (highlightCell != null) highlightCell.gameObject.SetActive(active);
        }

        public void HighlightSlot(int index)
        {
            if (slotRenderers == null) CacheRenderers();
            for (int i = 0; i < slotRenderers.Length; i++)
            {
                SpriteRenderer r = slotRenderers[i];
                if (r == null) continue;
                r.color = i == index ? HighlightColor : NormalColor;
            }
        }

        private void SetAllNormal()
        {
            if (slotRenderers == null) return;
            foreach (SpriteRenderer r in slotRenderers) if (r != null) r.color = NormalColor;
        }

        private static Color ParseColor(string html) => ColorUtility.TryParseHtmlString($"#{html}", out Color c) ? c : Color.white;

        public async UniTask BuildHeroes(int[] heroIds, int[] slotIndexes = null)
        {
            HeroConfig heroConfig = ConfigManager.Get<HeroConfig>();
            SkillConfig skillConfig = ConfigManager.Get<SkillConfig>();

            int count = Mathf.Min(heroIds.Length, slots.Length);

            for (int i = 0; i < count; i++)
            {
                int heroId = heroIds[i];
                int slotIndex = slotIndexes != null && i < slotIndexes.Length ? slotIndexes[i] : i;
                if (slotIndex < 0 || slotIndex >= slots.Length)
                {
                    Debug.LogWarning($"[{name}] Hero slot '{slotIndex}' is outside the configured slots");
                    continue;
                }

                if (heroId <= 0) continue; // ô trống

                if (!heroConfig.TryGetHero(heroId, out HeroConfigData heroData))
                {
                    Debug.LogWarning($"[{name}] Hero id '{heroId}' not found in HeroConfig");
                    continue;
                }

                GameObject prefab = await AssetLoader.GetAssetCached<GameObject>(heroData.prefabName);
                if (prefab == null)
                {
                    Debug.LogWarning($"[{name}] Hero prefab '{heroData.prefabName}' not found");
                    continue;
                }

                var go = Instantiate(prefab, slots[slotIndex].transform);
                if (!go.TryGetComponent<Hero>(out var hero))
                {
                    Debug.LogError($"[{name}] Prefab '{heroData.prefabName}' thiếu component Hero trên root");
                    Destroy(go); continue;
                }
                hero.transform.localPosition = Vector3.zero;
                hero.SetCircuitSlotIndex(slotIndex);

                // skill cấu hình theo attackId
                if (!skillConfig.TryGetSkill(heroData.attackId, out SkillConfigData skillData))
                {
                    Debug.LogWarning($"[{name}] Hero '{heroId}' attack skill '{heroData.attackId}' not found in SkillConfig");
                    Pool.Destroy(hero.gameObject);
                    continue;
                }

                hero.Initialize(heroData, skillData);
                hero.SetPowerUpgrades(skillConfig.GetStatUpgrades(heroData.listStatUpgrade));
            }
        }
    }
}

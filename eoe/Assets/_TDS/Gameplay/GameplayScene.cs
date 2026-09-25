using System;
using System.Collections.Generic;
using System.Diagnostics;
using _GameToolkit.GameConfig;
using _GameToolkit.ResourceManagement;
using _GameToolkit.Startup;
using _GameToolkit.Updater;
using _TDS.Battle;
using _TDS.GameConfig;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using _TDS.Utils;

namespace _TDS.Gameplay
{
    [Serializable]
    public struct CircuitDesignerSlot
    {
        [Range(0, 7)] public int slotIndex;
        public CircuitSlotContentType contentType;
        [Tooltip("HeroId nếu là Hero, ItemId nếu là Item")] public int contentId;
        public CircuitItemType itemType;
        [Min(1)] public int itemPower;
        [Min(0f)] public float powerDuration;
    }

    public class GameplayScene : MonoBehaviour
    {
        [Header("Circuit Designer")]
        [Tooltip("Designer kéo thả slot/index trực tiếp")]
        [SerializeField] private List<CircuitDesignerSlot> designerSlots = new List<CircuitDesignerSlot>();
#if UNITY_EDITOR
        [Header("Editor")]
        [Tooltip("Load local assets and configs when entering this scene directly from the Editor.")]
        [SerializeField] private bool loadDirectlyInEditor;
#endif
        [SerializeField] private HeroSlotManager heroSlotManager;
        [SerializeField] private UpdateRunner runner;
        [SerializeField] private CircuitTickRunner circuitRunner;
        [SerializeField] private WaveUpgradePanel upgradePanel;

        private RoundTimeline roundTimeline;
        private SpawnMonsterRunner spawnRunner;
        private AgentMovementRunner agentRunner;
        private SkillTickRunner skillRunner;
        private CircuitBoard board;
        private SkillConfig skillConfig;
        private ExpConfig expConfig;
        private int level = 1;
        private readonly BattleRunRewards rewards = new BattleRunRewards();
        private readonly List<Hero> trackedHeroes = new List<Hero>();
        private int normalKills;
        private int eliteKills;
        private int bossKills;
        private bool resultReported;

        public CircuitBoard Board => board;
        public BattleRunRewards Rewards => rewards;
        public RoundTimeline Timeline => roundTimeline;
        
        private void Awake()
        {
			Debug.LogError("Sửa lại thành game góc nhìn giống Plant vs Zombie. Reskin Plant luôn");
            GameObject timelineRoot = GameObject.Find("Canvas/top/bg");
            if (timelineRoot != null)
            {
                roundTimeline = timelineRoot.GetComponent<RoundTimeline>();
                if (roundTimeline == null) roundTimeline = timelineRoot.AddComponent<RoundTimeline>();
            }

            level = RunSelection.SelectedLevel;
            board = BuildBoard();
            if (upgradePanel == null)
            {
                upgradePanel = GetComponent<WaveUpgradePanel>();
            }
            if (upgradePanel == null || !upgradePanel.IsConfigured)
            {
                Debug.LogError("[Gameplay] WaveUpgradePanel chưa được cấu hình. Chạy TDS/Configure Gameplay UI.");
            }

            runner.TryGetRunner(out spawnRunner);
            runner.TryGetRunner(out agentRunner);
            runner.TryGetRunner(out skillRunner);
            runner.TryGetRunner(out circuitRunner);
        }

        private void Update()
        {
            EnergyCircuit circuit = circuitRunner?.Circuit;
            heroSlotManager?.RefreshHighlight(circuit);
        }

        private CircuitBoard BuildBoard()
        {
            if (designerSlots == null || designerSlots.Count == 0)
            {
                Debug.LogWarning("[CircuitDesigner] designerSlots trống - board rỗng");
                return new CircuitBoard(4);
            }
            int maxIndex = 0;
            foreach (var s in designerSlots) maxIndex = Math.Max(maxIndex, s.slotIndex);
            int boardSize = Mathf.Clamp(maxIndex + 1, 1, EnergyCircuit.DefaultSlotCount);
            CircuitBoard b = new CircuitBoard(boardSize);
            HashSet<int> used = new HashSet<int>();
            foreach (CircuitDesignerSlot s in designerSlots)
            {
                if (s.slotIndex < 0 || s.slotIndex >= b.SlotCount)
                {
                    Debug.LogWarning($"[CircuitDesigner] slotIndex {s.slotIndex} ngoài 0..{b.SlotCount - 1}, bỏ qua");
                    continue;
                }
                if (!used.Add(s.slotIndex))
                {
                    Debug.LogWarning($"[CircuitDesigner] slotIndex {s.slotIndex} trùng, bỏ qua");
                    continue;
                }
                if (s.contentType == CircuitSlotContentType.Empty) b.ClearSlot(s.slotIndex);
                else if (s.contentType == CircuitSlotContentType.Hero)
                {
                    if (s.contentId <= 0) { Debug.LogWarning($"[CircuitDesigner] Hero slot {s.slotIndex} thiếu contentId"); continue; }
                    b.SetContent(s.slotIndex, CircuitSlotContent.Hero(s.contentId));
                }
                else // Item
                {
                    if (s.contentId <= 0 || s.itemType == CircuitItemType.None) { Debug.LogWarning($"[CircuitDesigner] Item slot {s.slotIndex} thiếu contentId/itemType"); continue; }
                    b.SetItem(s.slotIndex, s.contentId, s.itemType, Mathf.Max(1, s.itemPower));
                    if (s.powerDuration > 0f) b.SetPowerDuration(s.slotIndex, s.powerDuration);
                }
            }
            return b;
        }

        private void OnEnable()
        {
            runner.OnPauseChanged += PauseChanged;
            runner.OnTimeScaleChanged += TimeScaleChanged;
            circuitRunner.OnActivation += OnCircuitActivation;
            Monster.OnMonsterRewarded += OnMonsterRewarded;
            Hero.OnHeroEnable += TrackHero;
        }

        private void OnDisable()
        {
            runner.OnPauseChanged -= PauseChanged;
            runner.OnTimeScaleChanged -= TimeScaleChanged;
            circuitRunner.OnActivation -= OnCircuitActivation;
            Monster.OnMonsterRewarded -= OnMonsterRewarded;
            Hero.OnHeroEnable -= TrackHero;
        }

        private async void Start()
        {
            Stopwatch sw = Stopwatch.StartNew();

            PlayerVitals.Reset();

#if UNITY_EDITOR
            if (loadDirectlyInEditor)
            {
                AssetLoader.SetAssetLocal();
                GameProgress.Load();
                await ConfigManager.Load(new[]
                {
                    nameof(MonsterConfig),
                    nameof(SpawnConfig),
                    nameof(SkillConfig),
                    nameof(ExpConfig),
                    nameof(HeroConfig),
                });
            }
#endif

            agentRunner.Initialize();
            circuitRunner.Initialize(board);

            skillRunner.Initialize();
            skillConfig = ConfigManager.Get<SkillConfig>();
            expConfig = ConfigManager.Get<ExpConfig>();
            SkillFactory.Initialize(skillRunner.Unit);

            // theo dõi wave: spawn xong -> chờ kill all -> wave mới -> hết wave -> win
            spawnRunner.OnSpawnCompleted += OnWaveSpawnCompleted;
            spawnRunner.OnWaveCleared += OnWaveCleared;
            spawnRunner.OnGameWin += OnGameWin;

            // hp chung cạn -> thua
            PlayerVitals.OnDied += OnPlayerDied;

            await spawnRunner.LoadLevelAsync(agentRunner, level);

            List<int> ids = new List<int>();
            List<int> slotIndexes = new List<int>();
            for (int i = 0; i < board.SlotCount; i++)
            {
                if (board.GetContent(i).Type != CircuitSlotContentType.Hero) continue;
                ids.Add(board.GetContent(i).Id);
                slotIndexes.Add(i);
            }

            await heroSlotManager.BuildHeroes(ids.ToArray(), slotIndexes.ToArray());
            
            sw.Stop();
            
            Debug.Log($"Gameplay init in {sw.ElapsedMilliseconds}ms");
            
            BootScene.Instance?.CloseLoadingScene();

            runner.IsPaused = false;
        }

        private void OnDestroy()
        {
            PlayerVitals.OnDied -= OnPlayerDied;
            trackedHeroes.Clear();

            if (spawnRunner != null)
            {
                spawnRunner.OnSpawnCompleted -= OnWaveSpawnCompleted;
                spawnRunner.OnWaveCleared -= OnWaveCleared;
                spawnRunner.OnGameWin -= OnGameWin;
            }

            spawnRunner?.Dispose();
            agentRunner?.Dispose();
            SkillFactory.Dispose();
        }

        private void OnCircuitActivation(CircuitActivationEvent activation)
        {
            if (activation.Content.Type != CircuitSlotContentType.Hero)
            {
                return;
            }

            foreach (Hero hero in Hero.AliveHeroes)
            {
                if (hero.CircuitSlotIndex != activation.SlotIndex) continue;
                if (hero.HeroId != activation.Content.Id) continue;
                hero.TryStartOverdrive(activation.PowerDuration);
            }
        }

        private void OnWaveSpawnCompleted(int wave)
        {
            Debug.Log($"[Gameplay] Wave {wave} spawn xong, chờ diệt hết quái...");
        }

        private void OnWaveCleared(int wave)
        {
            if (upgradePanel == null || !upgradePanel.IsConfigured)
            {
                Debug.LogError("[Gameplay] Không thể mở upgrade popup vì WaveUpgradePanel chưa được cấu hình.");
                ResumeUpgradeFlow();
                return;
            }

            PauseUpgradeFlow();
            bool statUpgradeWave = wave % 2 == 1;
            if (statUpgradeWave)
            {
                ShowUpgradeRoll();
            }
            else
            {
                ShowShop();
            }

            Debug.Log($"[Gameplay] Diệt hết quái wave {wave} -> mở {(statUpgradeWave ? "stat upgrade" : "core shop")}");
        }

        private void ShowShop()
        {
            PauseUpgradeFlow();
            upgradePanel.ShowCards(
                "CORE SHOP",
                "BUY ONE CORE WITH RUN GOLD",
                rewards.Gold,
                PickCards(skillConfig.ShopItems, 3),
                requiresGold: true,
                ApplyUpgradeCard);
        }

        private void ShowUpgradeRoll()
        {
            PauseUpgradeFlow();
            List<UpgradeCardConfigData> candidates = new List<UpgradeCardConfigData>();
            candidates.AddRange(skillConfig.HeroUpgrades);

            HashSet<int> skillIds = new HashSet<int>();
            foreach (Hero hero in trackedHeroes)
            {
                if (hero != null && hero.SkillId > 0) skillIds.Add(hero.SkillId);
            }

            foreach (int skillId in skillIds)
            {
                candidates.AddRange(skillConfig.GetSkillPool(skillId));
            }

            upgradePanel.ShowCards(
                "UPGRADE ROLL",
                "ROLL 3 CARDS · CHOOSE ONE",
                rewards.Gold,
                PickCards(candidates, 3),
                requiresGold: false,
                ApplyUpgradeCard);
        }

        private void ApplyUpgradeCard(UpgradeCardConfigData card)
        {
            if (card.kind == UpgradeCardKind.ShopItem && !rewards.TrySpendGold(card.cost))
            {
                return;
            }

            bool applied = false;
            foreach (Hero hero in trackedHeroes)
            {
                if (hero == null) continue;
                if (card.kind == UpgradeCardKind.SkillStat)
                {
                    if (hero.SkillId == card.skillId)
                    {
                        applied |= hero.ApplySkillUpgrade(card.skillStat, card.value, card.percent);
                    }
                }
                else if (card.heroId == 0 || hero.HeroId == card.heroId)
                {
                    applied |= hero.ApplyHeroUpgrade(card.heroStat, card.value, card.percent);
                }
            }

            if (!applied)
            {
                if (card.kind == UpgradeCardKind.ShopItem) rewards.RefundGold(card.cost);
                Debug.LogWarning($"[Gameplay] Upgrade card '{card.id}' did not match a hero");
                return;
            }

            rewards.RecordUpgrade(card.id);
            upgradePanel.Hide();
            ResumeUpgradeFlow();
        }

        private void PauseUpgradeFlow()
        {
            runner.IsPaused = true;
            if (spawnRunner != null) spawnRunner.IsPaused = true;
        }

        private void ResumeUpgradeFlow()
        {
            if (spawnRunner != null) spawnRunner.IsPaused = false;
            runner.IsPaused = false;
        }

        private static List<UpgradeCardConfigData> PickCards(
            IReadOnlyList<UpgradeCardConfigData> source,
            int count)
        {
            List<UpgradeCardConfigData> pool = new List<UpgradeCardConfigData>(source ?? Array.Empty<UpgradeCardConfigData>());
            List<UpgradeCardConfigData> result = new List<UpgradeCardConfigData>();
            int amount = Mathf.Min(count, pool.Count);
            for (int i = 0; i < amount; i++)
            {
                int index = GameRng.Range(0, pool.Count);
                result.Add(pool[index]);
                pool.RemoveAt(index);
            }

            return result;
        }

        private void ContinueAfterResult(bool victory)
        {
            RunSelection.SelectLevel(victory
                ? Mathf.Min(RunSelection.MaxCampaignLevel, level + 1)
                : level);
            LoadScene("GamePlayScene");
        }

        private void ReturnHome()
        {
            LoadScene("HomeScene");
        }

        private static void LoadScene(string sceneName)
        {
            if (BootScene.Instance == null)
            {
                SceneManager.LoadScene(sceneName);
                return;
            }

            BootScene.Instance.LoadSceneAsync(sceneName);
            BootScene.Instance.CloseLoadingScene();
        }

        private void OnGameWin()
        {
            upgradePanel?.Hide();
            if (resultReported) return;
            resultReported = true;
            int[] saveIds = GetSaveIds();
            GameProgress.SaveRun(level, saveIds, rewards, victory: true, expConfig: expConfig);
            Debug.Log($"[Gameplay] 🏆 WIN GAME! EXP={rewards.Experience}, GOLD={rewards.Gold}");
            LogBattleReport("WIN");
        }

        private void OnMonsterRewarded(Monster monster, int experience, int gold)
        {
            rewards.Add(experience, gold);
            circuitRunner?.AddKillEnergy();
            switch (monster.Rank)
            {
                case MonsterRank.Elite: eliteKills++; break;
                case MonsterRank.Boss: bossKills++; break;
                default: normalKills++; break;
            }

            Debug.Log($"[Gameplay] Reward monster={monster.name}: EXP +{experience}, GOLD +{gold}");
        }

        private void TrackHero(Hero hero)
        {
            if (hero != null && !trackedHeroes.Contains(hero)) trackedHeroes.Add(hero);
        }

        private void LogBattleReport(string outcome)
        {
            Debug.Log($"[BattleReport] outcome={outcome}");
            foreach (Hero hero in trackedHeroes)
            {
                Debug.Log($"[BattleReport] hero={hero.HeroId} totalDamage={hero.TotalDamageDealt}");
            }

            Debug.Log($"[BattleReport] monsters normal={normalKills} elite={eliteKills} boss={bossKills}");
        }

        private void OnPlayerDied()
        {
            Debug.Log("[Gameplay] Player hết máu!");

            if (resultReported) return;

            resultReported = true;
            upgradePanel?.Hide();
            int[] loseIds = GetSaveIds();
            GameProgress.SaveRun(level, loseIds, rewards, victory: false, expConfig: expConfig);
            Debug.Log($"[Gameplay] 💀 THUA! EXP={rewards.Experience}, GOLD={rewards.Gold}");
            LogBattleReport("LOSE");
        }

        private void TimeScaleChanged(float deltaTime)
        {
        }

        private void PauseChanged(bool paused)
        {
        }

        private int[] GetSaveIds()
        {
            List<int> ids = new List<int>();
            if (board != null)
                for (int i = 0; i < board.SlotCount; i++)
                    if (board.GetContent(i).Type == CircuitSlotContentType.Hero)
                        ids.Add(board.GetContent(i).Id);
            return ids.ToArray();
        }
    }
}

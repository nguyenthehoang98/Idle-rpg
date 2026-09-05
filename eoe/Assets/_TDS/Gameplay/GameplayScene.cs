using System;
using System.Collections.Generic;
using System.Diagnostics;
using _GameToolkit.GameConfig;
using _GameToolkit.Startup;
using _GameToolkit.Updater;
using _TDS.Battle;
using _TDS.GameConfig;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace _TDS.Gameplay
{
    public class GameplayScene : MonoBehaviour
    {
        [SerializeField] private int[] heroIds = new int[4] { 101, 102, 0, 0 };
        [SerializeField] private HeroSlotManager heroSlotManager;
        [SerializeField] private UpdateRunner runner;
        [SerializeField] private CircuitTickRunner circuitRunner;

        private SpawnMonsterRunner spawnRunner;
        private AgentMovementRunner agentRunner;
        private SkillTickRunner skillRunner;
        private CircuitBoard board;
        private GameplayHud hud;
        private WaveUpgradePanel upgradePanel;
        private UpgradeConfig upgradeConfig;
        private int level = 1;
        private readonly BattleRunRewards rewards = new BattleRunRewards();
        private readonly List<Hero> trackedHeroes = new List<Hero>();
        private int normalKills;
        private int eliteKills;
        private int bossKills;
        private bool resultReported;

        public CircuitBoard Board => board;
        public BattleRunRewards Rewards => rewards;
        
        private void Awake()
        {
            level = RunSelection.SelectedLevel;
            board = CircuitBoard.FromHeroesWithStarterGenerator(heroIds);
            hud = gameObject.GetComponent<GameplayHud>();
            if (hud == null)
            {
                hud = gameObject.AddComponent<GameplayHud>();
            }

            upgradePanel = gameObject.AddComponent<WaveUpgradePanel>();
            runner.TryGetRunner(out spawnRunner);
            runner.TryGetRunner(out agentRunner);
            runner.TryGetRunner(out skillRunner);
            runner.TryGetRunner(out circuitRunner);
        }

        private void OnEnable()
        {
            runner.OnPauseChanged += PauseChanged;
            runner.OnTimeScaleChanged += TimeScaleChanged;
            circuitRunner.OnActivation += OnCircuitActivation;
            Monster.OnMonsterRewarded += OnMonsterRewarded;
            Hero.OnHeroEnable += TrackHero;
            Monster.OnMonsterEnable += SubscribeMonsterModifierFeedback;
            Monster.OnMonsterDisable += UnsubscribeMonsterModifierFeedback;
        }

        private void OnDisable()
        {
            runner.OnPauseChanged -= PauseChanged;
            runner.OnTimeScaleChanged -= TimeScaleChanged;
            circuitRunner.OnActivation -= OnCircuitActivation;
            Monster.OnMonsterRewarded -= OnMonsterRewarded;
            Hero.OnHeroEnable -= TrackHero;
            Monster.OnMonsterEnable -= SubscribeMonsterModifierFeedback;
            Monster.OnMonsterDisable -= UnsubscribeMonsterModifierFeedback;
        }

        private async void Start()
        {
            Stopwatch sw = Stopwatch.StartNew();

            agentRunner.Initialize();
            circuitRunner.Initialize(board);
            hud.Initialize(board, level);
            hud.SetStatus("LOADING BATTLE");

            skillRunner.Initialize();
            upgradeConfig = ConfigManager.Get<UpgradeConfig>();
            SkillFactory.Initialize(skillRunner.Unit);

            // theo dõi wave: spawn xong -> chờ kill all -> wave mới -> hết wave -> win
            spawnRunner.OnSpawnCompleted += OnWaveSpawnCompleted;
            spawnRunner.OnWaveCleared += OnWaveCleared;
            spawnRunner.OnGameWin += OnGameWin;

            // theo dõi hero chết -> hết hero = thua
            Hero.OnHeroDisable += OnHeroDied;

            await spawnRunner.LoadLevelAsync(agentRunner, level);

            await heroSlotManager.BuildHeroes(heroIds);
            
            sw.Stop();
            
            Debug.Log($"Gameplay init in {sw.ElapsedMilliseconds}ms");
            
            BootScene.Instance.CloseLoadingScene();

            runner.IsPaused = false;
        }

        private void OnDestroy()
        {
            Hero.OnHeroDisable -= OnHeroDied;
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
            hud.ActivateSlot(activation.SlotIndex);

            if (activation.Content.Type != CircuitSlotContentType.Hero)
            {
                return;
            }

            foreach (Hero hero in Hero.AliveHeroes)
            {
                if (hero.HeroId == activation.Content.Id && hero.TryStartOverdrive(EnergyCircuit.DefaultOverdriveDuration))
                {
                    hud.SetStatus("OVERDRIVE ACTIVE");
                }
            }
        }

        private void OnWaveSpawnCompleted(int wave)
        {
            hud.SetWave(wave);
            hud.SetStatus("FIGHTING");
            Debug.Log($"[Gameplay] Wave {wave} spawn xong, chờ diệt hết quái...");
        }

        private void OnWaveCleared(int wave)
        {
            hud.SetWave(wave);
            hud.SetStatus("CHOOSE UPGRADE");
            PauseUpgradeFlow();
            upgradePanel.ShowChoice(wave, rewards.Gold, ShowShop, ShowUpgradeRoll);
            Debug.Log($"[Gameplay] Diệt hết quái wave {wave} -> mở upgrade choice");
        }

        private void ShowShop()
        {
            PauseUpgradeFlow();
            upgradePanel.ShowCards(
                "GOLD SHOP",
                "BUY ONE ITEM WITH RUN GOLD",
                rewards.Gold,
                PickCards(upgradeConfig.ShopItems, 3),
                requiresGold: true,
                ApplyUpgradeCard);
        }

        private void ShowUpgradeRoll()
        {
            PauseUpgradeFlow();
            List<UpgradeCardConfigData> candidates = new List<UpgradeCardConfigData>();
            candidates.AddRange(upgradeConfig.HeroUpgrades);

            HashSet<int> skillIds = new HashSet<int>();
            foreach (Hero hero in trackedHeroes)
            {
                if (hero != null && hero.SkillId > 0) skillIds.Add(hero.SkillId);
            }

            foreach (int skillId in skillIds)
            {
                candidates.AddRange(upgradeConfig.GetSkillPool(skillId));
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
            hud.SetStatus($"UPGRADE: {card.title}");
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
                int index = UnityEngine.Random.Range(0, pool.Count);
                result.Add(pool[index]);
                pool.RemoveAt(index);
            }

            return result;
        }

        private void OnGameWin()
        {
            upgradePanel?.Hide();
            if (resultReported) return;
            resultReported = true;
            GameProgress.SaveRun(level, heroIds, rewards, victory: true);
            hud.SetStatus($"VICTORY  +{rewards.Experience} EXP  +{rewards.Gold} GOLD");
            hud.ShowResult(true, rewards);
            Debug.Log($"[Gameplay] 🏆 WIN GAME! EXP={rewards.Experience}, GOLD={rewards.Gold}");
            LogBattleReport("WIN");
        }

        private void OnMonsterRewarded(Monster monster, int experience, int gold)
        {
            rewards.Add(experience, gold);
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

        private void SubscribeMonsterModifierFeedback(Monster monster)
        {
            if (monster != null)
            {
                monster.OnModifierApplied += OnModifierApplied;
            }
        }

        private void UnsubscribeMonsterModifierFeedback(Monster monster)
        {
            if (monster != null)
            {
                monster.OnModifierApplied -= OnModifierApplied;
            }
        }

        private void OnModifierApplied(SkillModifierType type)
        {
            hud?.ShowModifierFeedback(type);
        }

        private void OnHeroDied(Hero hero)
        {
            // chỉ xử lý khi hero thực sự chết (OnHeroDisable cũng fire khi scene off/pool)
            if (hero == null || !hero.IsDead) return;

            Debug.Log($"[Gameplay] Hero {hero.name} chết!");

            // hết hero sống -> thua
            int alive = 0;
            foreach (Hero h in Hero.AliveHeroes)
            {
                if (h != null && !h.IsDead) alive++;
            }

            if (alive == 0 && !resultReported)
            {
                upgradePanel?.Hide();
                resultReported = true;
                GameProgress.SaveRun(level, heroIds, rewards, victory: false);
                hud.SetStatus($"DEFEAT  +{rewards.Experience} EXP  +{rewards.Gold} GOLD");
                hud.ShowResult(false, rewards);
                Debug.Log($"[Gameplay] 💀 THUA! EXP={rewards.Experience}, GOLD={rewards.Gold}");
                LogBattleReport("LOSE");
            }
        }

        private void TimeScaleChanged(float deltaTime)
        {
        }

        private void PauseChanged(bool paused)
        {
        }
    }
}

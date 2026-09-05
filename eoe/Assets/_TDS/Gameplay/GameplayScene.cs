using System.Diagnostics;
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
        [SerializeField] private int[] heroIds = new int[4] { 101, 0, 0, 0 };
        [SerializeField] private HeroSlotManager heroSlotManager;
        [SerializeField] private UpdateRunner runner;
        [SerializeField] private CircuitTickRunner circuitRunner;

        private SpawnMonsterRunner spawnRunner;
        private AgentMovementRunner agentRunner;
        private SkillTickRunner skillRunner;
        private CircuitBoard board;
        private GameplayHud hud;
        private int level = 1;
        private readonly BattleRunRewards rewards = new BattleRunRewards();

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
            Monster.OnMonsterEnable += SubscribeMonsterModifierFeedback;
            Monster.OnMonsterDisable += UnsubscribeMonsterModifierFeedback;
        }

        private void OnDisable()
        {
            runner.OnPauseChanged -= PauseChanged;
            runner.OnTimeScaleChanged -= TimeScaleChanged;
            circuitRunner.OnActivation -= OnCircuitActivation;
            Monster.OnMonsterRewarded -= OnMonsterRewarded;
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
            hud.SetStatus("WAVE CLEARED");
            Debug.Log($"[Gameplay] Diệt hết quái wave {wave} -> wave mới");
        }

        private void OnGameWin()
        {
            hud.SetStatus($"VICTORY  +{rewards.Experience} EXP  +{rewards.Gold} GOLD");
            hud.ShowResult(true, rewards);
            Debug.Log($"[Gameplay] 🏆 WIN GAME! EXP={rewards.Experience}, GOLD={rewards.Gold}");
        }

        private void OnMonsterRewarded(Monster monster, int experience, int gold)
        {
            rewards.Add(experience, gold);
            Debug.Log($"[Gameplay] Reward monster={monster.name}: EXP +{experience}, GOLD +{gold}");
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

            if (alive == 0)
            {
                hud.SetStatus($"DEFEAT  +{rewards.Experience} EXP  +{rewards.Gold} GOLD");
                hud.ShowResult(false, rewards);
                Debug.Log($"[Gameplay] 💀 THUA! EXP={rewards.Experience}, GOLD={rewards.Gold}");
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

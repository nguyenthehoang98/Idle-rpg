using System.Diagnostics;
using _GameToolkit.Startup;
using _GameToolkit.Updater;
using _TDS.Battle;
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
        private int level = 1;

        public CircuitBoard Board => board;
        
        private void Awake()
        {
            board = CircuitBoard.FromHeroes(heroIds);

            runner.TryGetRunner(out spawnRunner);
            runner.TryGetRunner(out agentRunner);
            runner.TryGetRunner(out skillRunner);
            runner.TryGetRunner(out circuitRunner);
        }

        private void OnEnable()
        {
            runner.OnPauseChanged += PauseChanged;
            runner.OnTimeScaleChanged += TimeScaleChanged;
        }

        private void OnDisable()
        {
            runner.OnPauseChanged -= PauseChanged;
            runner.OnTimeScaleChanged -= TimeScaleChanged;
        }

        private async void Start()
        {
            Stopwatch sw = Stopwatch.StartNew();

            agentRunner.Initialize();
            circuitRunner.Initialize(board);

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

        private void OnWaveSpawnCompleted(int wave) =>
            Debug.Log($"[Gameplay] Wave {wave} spawn xong, chờ diệt hết quái...");

        private void OnWaveCleared(int wave) =>
            Debug.Log($"[Gameplay] Diệt hết quái wave {wave} -> wave mới");

        private void OnGameWin() =>
            Debug.Log("[Gameplay] 🏆 WIN GAME! Diệt hết toàn bộ quái vật");

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
                Debug.Log("[Gameplay] 💀 THUA! Toàn bộ hero đã chết");
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

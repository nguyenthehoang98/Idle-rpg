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
        
        private SpawnMonsterRunner spawnRunner;
        private AgentMovementRunner agentRunner;
        private SkillTickRunner skillRunner;
        private int level = 1;
        
        private void Awake()
        {
            runner.TryGetRunner(out spawnRunner);
            runner.TryGetRunner(out agentRunner);
            runner.TryGetRunner(out skillRunner);
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
            
            skillRunner.Initialize();
            
            SkillFactory.Initialize(skillRunner.Unit);

            // theo dõi wave: spawn xong -> chờ kill all -> wave mới -> hết wave -> win
            spawnRunner.OnSpawnCompleted += OnWaveSpawnCompleted;
            spawnRunner.OnWaveCleared += OnWaveCleared;
            spawnRunner.OnGameWin += OnGameWin;

            await spawnRunner.LoadLevelAsync(agentRunner, level);

            await heroSlotManager.BuildHeroes(heroIds);
            
            sw.Stop();
            
            Debug.Log($"Gameplay init in {sw.ElapsedMilliseconds}ms");
            
            BootScene.Instance.CloseLoadingScene();

            runner.IsPaused = false;
        }

        private void OnDestroy()
        {
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

        private void TimeScaleChanged(float deltaTime)
        {
        }

        private void PauseChanged(bool paused)
        {
        }
    }
}

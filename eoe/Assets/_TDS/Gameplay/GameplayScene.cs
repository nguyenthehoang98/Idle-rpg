using System.Diagnostics;
using _GameToolkit.Startup;
using _GameToolkit.Updater;
using _TDS.Battle;
using Cysharp.Threading.Tasks;
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
        private int level = 1;
        
        private void Awake()
        {
            runner.TryGetRunner(out spawnRunner);
            runner.TryGetRunner(out agentRunner);
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

            await spawnRunner.LoadLevelAsync(agentRunner, level);

            await BuildHeroes();
            
            sw.Stop();
            
            Debug.Log($"Gameplay init in {sw.ElapsedMilliseconds}ms");
            
            BootScene.Instance.CloseLoadingScene();
        }

        private UniTask BuildHeroes()
        {
            return heroSlotManager.BuildHeroes(heroIds);
        }

        private void OnDestroy()
        {
            spawnRunner.Dispose();
            agentRunner.Dispose();
        }

        private void TimeScaleChanged(float deltaTime)
        {
        }

        private void PauseChanged(bool paused)
        {
        }
    }
}

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

            await spawnRunner.LoadLevelAsync(agentRunner, level);

            await heroSlotManager.BuildHeroes(heroIds);
            
            sw.Stop();
            
            Debug.Log($"Gameplay init in {sw.ElapsedMilliseconds}ms");
            
            BootScene.Instance.CloseLoadingScene();

            runner.IsPaused = false;
        }

        private void OnDestroy()
        {
            spawnRunner.Dispose();
            agentRunner.Dispose();
            SkillFactory.Dispose();
        }

        private void TimeScaleChanged(float deltaTime)
        {
        }

        private void PauseChanged(bool paused)
        {
        }
    }
}

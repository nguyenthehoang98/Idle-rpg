using System.Diagnostics;
using _GameToolkit.Startup;
using _GameToolkit.Updater;
using _TDS.Battle;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace _TDS.Gameplay
{
    [RequireComponent(typeof(UpdateRunner))]
    public class GameplayScene : MonoBehaviour
    {
        private UpdateRunner runner;
        private SpawnMonsterRunner spawnRunner;
        private AgentMovementRunner agentRunner;
        private int level = 1;

        private void Awake()
        {
            runner = GetComponent<UpdateRunner>();
            runner.TryGetRunner(out spawnRunner);
            runner.TryGetRunner(out agentRunner);
        }

        private void OnEnable()
        {
            runner.OnPauseChanged += PauseChanged;
            runner.OnTimeScaleChanged += TimeScaleChanged;
            /*Monster.OnMonsterEnable += MonsterEnable;
            Monster.OnMonsterDisable += MonsterDisable;*/
        }

        private void OnDisable()
        {
            runner.OnPauseChanged -= PauseChanged;
            runner.OnTimeScaleChanged -= TimeScaleChanged;
            /*Monster.OnMonsterEnable -= MonsterEnable;
            Monster.OnMonsterDisable -= MonsterDisable;*/
        }

        private async void Start()
        {
            Stopwatch sw = Stopwatch.StartNew();

            agentRunner.Initialize();

            await spawnRunner.LoadLevelAsync(agentRunner, level);
            
            sw.Stop();
            
            Debug.Log($"Gameplay init in {sw.ElapsedMilliseconds}ms");
            
            BootScene.Instance.CloseLoadingScene();
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

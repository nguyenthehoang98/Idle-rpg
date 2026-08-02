using _TDS.GameplayScene.Spawn;
using _Toolkit.Updater;
using UnityEngine;

namespace _TDS.GameplayScene
{
    public class GameplayStartup : MonoBehaviour
    {
        [SerializeField] private UpdateRunner runner;

        SpawnMonsterTickRunner tickRunner;

        private void Awake()
        {
            runner.TryGetRunner(out tickRunner);
            tickRunner.OnWaveSpawned += OnWaveSpawner;
        }

        private async void Start()
        {
            await tickRunner.Initialize(1);
            runner.IsPaused = false;
        }

        private void OnDestroy()
        {
            tickRunner.OnWaveSpawned -= OnWaveSpawner;
        }

        private void OnWaveSpawner(int wave)
        {
        }
    }
}
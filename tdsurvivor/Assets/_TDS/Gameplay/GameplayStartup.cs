using _GameToolkit.Updater;
using UnityEngine;

namespace _TDS.Gameplay
{
    public class GameplayStartup : MonoBehaviour
    {
        [SerializeField] private UpdaterOwner updaterOwner;

        SpawnerUpdater spawnerUpdater;
        
        private void Awake()
        {
            updaterOwner.TryGet(out spawnerUpdater);
            spawnerUpdater.OnWaveSpawned += OnWaveSpawner;
        }

        private async void Start()
        {
            await spawnerUpdater.Initialize(1);

            updaterOwner.IsPaused = false;
        }

        private void OnDestroy()
        {
            spawnerUpdater.OnWaveSpawned -= OnWaveSpawner;
        }

        private void OnWaveSpawner(int wave)
        {
        }
    }
}

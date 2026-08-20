using System;
using _TDS.Core;
using _TDS.GameplayScene.Spawn;
using _TDS.GameplayScene.Unit;
using UnityEngine;

namespace _TDS.GameplayScene
{
    public class WaveManager : MonoBehaviour
    {
        [SerializeField] private SpawnMonsterTickRunner spawner;
        [SerializeField] private int totalWaves = 10;

        private int currentWave;
        private int aliveMonsters;
        private bool allWavesSpawned;

        public int CurrentWave => currentWave;
        public int TotalWaves => totalWaves;
        public int AliveMonsters => aliveMonsters;

        public event Action<int> OnWaveCompleted;
        public event Action OnAllWavesCompleted;

        private void Awake()
        {
            if (spawner == null)
            {
                spawner = FindObjectOfType<SpawnMonsterTickRunner>();
            }

            if (spawner != null)
            {
                spawner.OnWaveSpawned += HandleWaveSpawned;
            }
        }

        public void Initialize(int totalWaves)
        {
            this.totalWaves = totalWaves;
            currentWave = 0;
            aliveMonsters = 0;
            allWavesSpawned = false;
        }

        private void HandleWaveSpawned(int wave)
        {
            currentWave = wave;
            OnWaveCompleted?.Invoke(wave);

            if (currentWave >= totalWaves)
            {
                allWavesSpawned = true;

                if (aliveMonsters <= 0)
                {
                    TriggerVictory();
                }
            }
        }

        public void OnMonsterSpawned()
        {
            aliveMonsters++;
        }

        public void OnMonsterDied()
        {
            aliveMonsters--;
            if (aliveMonsters < 0) aliveMonsters = 0;

            if (allWavesSpawned && aliveMonsters <= 0)
            {
                TriggerVictory();
            }
        }

        private void TriggerVictory()
        {
            if (GameManager.Instance != null && GameManager.Instance.State == GameState.Playing)
            {
                OnAllWavesCompleted?.Invoke();
                GameManager.Instance.SetState(GameState.Victory);
            }
        }

        private void OnDestroy()
        {
            if (spawner != null)
            {
                spawner.OnWaveSpawned -= HandleWaveSpawned;
            }
        }
    }
}

using _TDS.Core;
using _TDS.GameplayScene.Spawn;
using _TDS.GameplayScene.SkillSystem;
using _TDS.GameplayScene.Unit;
using _Toolkit.SkillSystem.Core;
using _Toolkit.Updater;
using UnityEngine;

namespace _TDS.GameplayScene
{
    public class GameplayStartup : MonoBehaviour
    {
        [SerializeField] private UpdateRunner runner;
        [SerializeField] private WaveManager waveManager;

        SpawnMonsterTickRunner spawnerTickRunner;
        MonsterTickRunner monsterTickRunner;
        SkillTickRunner skillTickRunner;

        private void Awake()
        {
            if (runner == null) return;

            runner.TryGetRunner(out spawnerTickRunner);
            runner.TryGetRunner(out monsterTickRunner);
            runner.TryGetRunner(out skillTickRunner);

            if (skillTickRunner != null)
            {
                SkillFactory.Initialize(skillTickRunner);
            }

            if (spawnerTickRunner != null)
            {
                spawnerTickRunner.OnWaveSpawned += OnWaveSpawner;
                spawnerTickRunner.OnMonsterSpawned += OnMonsterSpawned;
            }

            if (monsterTickRunner != null)
            {
                monsterTickRunner.OnMonsterRemoved += OnMonsterRemoved;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.Playing);
            }

            if (waveManager == null)
            {
                waveManager = FindObjectOfType<WaveManager>();
            }
        }

        private async void Start()
        {
            if (runner == null) return;

            if (spawnerTickRunner != null)
            {
                await spawnerTickRunner.Initialize(1);

                // Tổng số wave lấy từ SpawnerConfig, không hardcode
                if (waveManager != null)
                {
                    waveManager.Initialize(spawnerTickRunner.MaxWave);
                }
            }

            runner.IsPaused = false;
        }

        private void OnDestroy()
        {
            if (spawnerTickRunner != null)
            {
                spawnerTickRunner.OnWaveSpawned -= OnWaveSpawner;
                spawnerTickRunner.OnMonsterSpawned -= OnMonsterSpawned;
            }

            if (monsterTickRunner != null)
            {
                monsterTickRunner.OnMonsterRemoved -= OnMonsterRemoved;
            }
        }

        private void OnWaveSpawner(int wave)
        {
        }

        private void OnMonsterSpawned()
        {
            if (waveManager != null)
            {
                waveManager.OnMonsterSpawned();
            }
        }

        private void OnMonsterRemoved()
        {
            if (waveManager != null)
            {
                waveManager.OnMonsterDied();
            }
        }
    }
}
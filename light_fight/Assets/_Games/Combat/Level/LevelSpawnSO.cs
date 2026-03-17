using System;
using System.Collections.Generic;
using _KIT.Resource;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace _Games.Combat.Level
{
    /// <summary>
    /// Điều tiết việc sinh quái vật dựa trên waves/batch/power/duration
    /// Input: số lượng waves, batch mỗi wave, power & duration mỗi batch, số enemy & rate random mỗi batch
    /// Output: So lượng enemy được sinh ra
    /// => Mục đích điều têít, visualize được soóng độ khó của level từ đó đưa ra kết quả chính xác hơn
    /// </summary>
   
    [CreateAssetMenu(menuName = "Level Spawn SO")]
    public class LevelSpawnSO : ScriptableObject
    {
        public float targetDuration;
        public LevelDesign design;
        public List<WaveSpawn> waves = new List<WaveSpawn>();

        private void OnValidate()
        {
            targetDuration = 0;
            foreach (var wave in waves)
            {
                foreach (var batch in wave.batches)
                {
                    targetDuration += batch.duration;
                }
            }
        }

        public void Validate()
        {
#if DEVELOP_MODE
            MonsterConfig monsterConfig = MonsterConfig.Instance;
            foreach (var wave in waves)
            {
                foreach (var batch in wave.batches)
                {
                    foreach (var enemy in batch.enemies)
                    {
                        if (monsterConfig.Find(enemy.id, out _)) continue;
                        Debug.LogError($"Không tìm thấy enemy với id '{enemy.id}' ở {name}");
                    }
                }
            }

            SpawnMonsterLogic.ValidateSpawn(this, monsterConfig, SkillConfig.Instance);
#endif
        }

        public static UniTask<LevelSpawnSO> LoadSpawn(int level)
        {
            return KitLoaded.LoadAsync<LevelSpawnSO>($"LevelSpawner_{level:D3}");
        }
        
        public void Disable()
        {
#if UNITY_EDITOR
            for (int w = 0; w < waves.Count; w++)
            {
                var wave = waves[w];
                if (wave.batches.Count == 0)
                {
                    wave.batches.Add(new BatchSpawn());
                }
                
                int power = wave.power;
                for (int b = 0; b < wave.batches.Count; b++)
                {
                    power -= wave.batches[b].power;
                }

                if (power > 0 && wave.batches.Count > 0)
                {
                    wave.batches[^1].power = power;
                }

                for (int i = 0; i < wave.batches.Count; i++)
                {
                    wave.batches[i].duration = math.max(1, wave.batches[i].duration);
                }
            }
#endif
        }

        [Serializable]
        public class WaveSpawn
        {
            public int power;
            public List<BatchSpawn> batches;
        }
        
        [Serializable]
        public class BatchSpawn
        {
            public bool isBoosWave;
            public float duration;
            public float waitTimeSpawn;
            public int power;
            public List<EnemySpawn> enemies;
        }
        
        [Serializable]
        public class EnemySpawn
        {
            public int id;
            public int level;
            public int weight;

            public EnemySpawn()
            {
                id = level = weight = 1;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using _Game.Battle.Systems;
using _Game.Configs;
using _KIT.Resource;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace _Game.Battle
{
    using UnityEngine;

    /// <summary>
    /// Điều tiết việc sinh quái vật dựa trên waves/batch/power/duration
    /// Input: số lượng waves, batch mỗi wave, power & duration mỗi batch, số enemy & rate random mỗi batch
    /// Output: So lượng enemy được sinh ra
    /// => Mục đích điều têít, visualize được soóng độ khó của level từ đó đưa ra kết quả chính xác hơn
    /// </summary>
    [CreateAssetMenu(fileName = "LevelSpawnConfig", menuName = "LevelSpawnConfig")]
    public class LevelSpawnConfig : ScriptableObject
    {
        public int targetDuration;
        public List<WaveSpawn> waves = new List<WaveSpawn>();

        public void Validate()
        {
#if DEVELOP_MODE
            MonsterConfig monsterConfig = MonsterConfig.Instance;
            SkillConfig skillConfig  = SkillConfig.Instance;
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
            SpawnMonsterSystem.ValidateSpawn(this, monsterConfig, skillConfig);
#endif
        }

        public static UniTask<LevelSpawnConfig> LoadSpawn(int level)
        {
            return KitLoaded.LoadAsync<LevelSpawnConfig>($"Spawner_{level}");
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
                    wave.batches[^1].power += power;
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
        }
    }
}
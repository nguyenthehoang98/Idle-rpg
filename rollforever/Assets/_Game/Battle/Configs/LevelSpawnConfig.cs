using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Serialization;

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
        public int maxAlive;
        public List<WaveSpawn> waves = new List<WaveSpawn>();
        
        [Serializable]
        public struct WaveSpawn
        {
            public int power;
            public bool isBoosWave;
            public List<BatchSpawn> batches;
        }
        
        [Serializable]
        public struct BatchSpawn
        {
            public float duration;
            public int powerBudget;
            public AnimationCurve curve;
            public List<EnemySpawnConfig> enemies;
        }
        
        [Serializable]
        public class EnemySpawnConfig
        {
            public int enemyId;
            public int2 enemyLevelRange;
            public int weight;
        }
    }
}
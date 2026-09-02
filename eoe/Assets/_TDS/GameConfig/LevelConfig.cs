using System;
using System.Collections.Generic;
using System.IO;
using _GameToolkit.GameConfig;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _TDS.GameConfig
{
    [Serializable]
    public class LevelConfig : IGameConfig
    {
        [SerializeField] private List<LevelData> levels = new List<LevelData>();
        [SerializeField] private List<SpawnData> spawns = new List<SpawnData>();

        private Dictionary<int, LevelData> cached;

        public void OnMappingValue()
        {
            cached = new Dictionary<int, LevelData>();

            foreach (var data in levels)
            {
                if (!cached.TryAdd(data.level, data)) Debug.LogError($"Duplicate level '{data.level}'");
            }
        }

        public void OnImported()
        {
            for (int i = 0; i < levels.Count; i++)
            {
                LevelData levelData = levels[i];

                List<SpawnData> list = new List<SpawnData>();
                
                foreach (var spawnData in spawns)
                {
                    if (spawnData.level == levelData.level) list.Add(spawnData);
                }
                
                levelData.spawns = list.ToArray();
                
                levels[i] = levelData;
            }
        }

        public void OnCompleteImported()
        {
#if UNITY_EDITOR
            string path = Path.Combine(ConfigPath.Folder, "MonsterConfig.json");
            
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (asset == null) return;
            
            MonsterConfig monsterConfig = JsonUtility.FromJson<MonsterConfig>(asset.text);

            monsterConfig.OnMappingValue();

            for (int i = 0; i < levels.Count; i++)
            {
                LevelData levelData = levels[i];

                foreach (var spawnData in levelData.spawns)
                {
                    if (monsterConfig.TryGetMonsterData(spawnData.monster, out MonsterData data)) continue;

                    Debug.LogError(
                        $"Not found monster '{spawnData.monster}', wave {spawnData.wave}, level {levelData.level} "
                    );
                }
            }
#endif
        }

        public bool TryGetLevelData(int levelId, out LevelData levelData)
        {
            return cached.TryGetValue(levelId, out levelData);
        }
    }

    [Serializable]
    public struct LevelData
    {
        public int level;
        public string backgroundPrefabName;
        public SpawnData[] spawns;
    }

    [Serializable]
    public struct SpawnData
    {
        public int level;
        public int wave;
        public int monster;
        public int total;
        public float attackScale;
        public float healthScale;
        public float expScale;
        public float scale;
        public float radius;
        public float startTime;
        public float endTime;
        public int[] portals;
    }
}

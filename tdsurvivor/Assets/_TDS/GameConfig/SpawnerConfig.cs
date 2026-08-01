using System;
using System.Collections.Generic;
using _Toolkit.Config;
using ExcelExtension;
using UnityEditor;
using UnityEngine;
namespace _TDS.GameConfig
{
    [Serializable, ExcelAsset(
         ExcelPath = "Assets/Excels/SpawnerConfig.xlsx",
         ConfigPath = "Assets/_TDS assets/Config/SpawnerConfig.json")]
    public class SpawnerConfig : IConfig
    {
        [SerializeField] private List<SpawnerConfigData> spawners = new List<SpawnerConfigData>();

        private Dictionary<int, List<SpawnerConfigData>> cachedSpawners;

        public void OnMappingValue()
        {
            cachedSpawners = new Dictionary<int, List<SpawnerConfigData>>();

            foreach (var spawner in spawners)
            {
                if (cachedSpawners.TryGetValue(spawner.level, out var list))
                {
                    list.Add(spawner);
                }
                else
                {
                    cachedSpawners.Add(spawner.level, new List<SpawnerConfigData> { spawner });
                }
            }
        }

        public void OnImported()
        {

        }

        public void OnCompleteImported()
        {
            Type type = typeof(MonsterConfig);

            object[] attributes = type.GetCustomAttributes(typeof(ExcelAssetAttribute), false);

            if (attributes.Length == 0) return;
            
            ExcelAssetAttribute attribute = (ExcelAssetAttribute)attributes[0];

            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(attribute.ConfigPath);

            MonsterConfig config = JsonUtility.FromJson<MonsterConfig>(asset.text);
            
            config.OnMappingValue();

            foreach (var spawner in spawners)
            {
                if (config.TryGetMonster(spawner.monster, out MonsterConfigData monster))
                {
                }
                else
                {
                    Debug.LogError($"Monster '{spawner.monster}' is not a valid monster at spawner level '{spawner.level}', wave '{spawner.wave}'");
                }
            }
        }

        public bool TryGetSpawner(int level, out List<SpawnerConfigData> configs)
        {
            return cachedSpawners.TryGetValue(level, out configs);
        }
    }

    [Serializable]
    public struct SpawnerConfigData
    {
        public int level;
        public int wave;
        public int monster;
        public int totalMonster;
        public float healthScale;
        public float attackScale;
        public float expScale;
        public float sizeScale;
        public float spawnStartTime;
        public float spawnEndTime;
        public float spawnAreaRadius;
        public int[] portals;
    }
}
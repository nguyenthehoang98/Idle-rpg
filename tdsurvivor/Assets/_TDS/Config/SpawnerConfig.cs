using System;
using System.Collections.Generic;
using _Toolkit.Config;
using ExcelExtension;
using UnityEditor;
using UnityEngine;

namespace _TDS.Config
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

            foreach (SpawnerConfigData spawner in spawners)
            {
                if (cachedSpawners.TryGetValue(spawner.level, out List<SpawnerConfigData> list))
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

            foreach (SpawnerConfigData spawner in spawners)
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
}
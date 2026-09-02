using System;
using System.Collections.Generic;
using System.IO;
using _GameToolkit.GameConfig;
using ExcelExtension;
using UnityEngine;

namespace _TDS.GameConfig
{
    [Serializable, ExcelAsset(
         ExcelPath = "Assets/Excels/SpawnConfig.xlsx",
         ConfigPath = "Assets/_TDSAssets/Config/SpawnConfig.json")]
    public class SpawnConfig : IConfig
    {
        [SerializeField] private List<SpawnConfigData> spawns = new List<SpawnConfigData>();

        private Dictionary<int, List<SpawnConfigData>> spawnCached;
        
        public void OnMappingValue()
        {
            spawnCached = new Dictionary<int, List<SpawnConfigData>>();

            foreach (var data in spawns)
            {
                int level = data.definition.level;
               
                if (spawnCached.TryGetValue(level, out var list))
                {
                    list.Add(data);        
                }
                else
                {
                    spawnCached.Add(level, new List<SpawnConfigData> { data });
                }
            }
        }

        public void OnImported()
        {
            for (int i = 0; i < spawns.Count; i++)
            {
                SpawnConfigData spawn = spawns[i];
                spawn.Parse();
                spawns[i] = spawn;
            }
        }

        public void OnCompleteImported()
        {
#if UNITY_EDITOR
            string path = Path.Combine(ConfigPath.Folder, nameof(MonsterConfig), ".json");
            
            TextAsset asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (asset == null) return;
            
            MonsterConfig monsterConfig = JsonUtility.FromJson<MonsterConfig>(asset.text);
            monsterConfig.OnMappingValue();

            foreach (var spawn in spawns)
            {
                int monsterId = spawn.monsterId;
                
                if (monsterConfig.TryGetMonsterData(monsterId, out MonsterData data)) continue;

                Debug.LogError($"Not found monster '{monsterId}', json '{JsonUtility.ToJson(spawn)}'");
            }
#endif
        }

        public bool TryGetSpawn(int level, out List<SpawnConfigData> data)
        {
            return spawnCached.TryGetValue(level, out data);
        }
    }
}
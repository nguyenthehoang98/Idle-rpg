using System;
using System.Collections.Generic;
using System.IO;
using _GameToolkit.GameConfig;
using ExcelExtension;
using Newtonsoft.Json;
using UnityEngine;

namespace _TDS.GameConfig
{
    [Serializable, ExcelAsset(
         ExcelPath = "Assets/Excels/SpawnConfig.xlsx",
         ConfigPath = "Assets/_TDSAssets/Config/SpawnConfig.json")]
    public class SpawnConfig : Config
    {
        [SerializeField, JsonProperty] private List<SpawnConfigData> spawns = new List<SpawnConfigData>();

        private Dictionary<int, List<SpawnConfigData>> cached;
        
        public override void OnMappingValue()
        {
            cached = new Dictionary<int, List<SpawnConfigData>>();

            foreach (var data in spawns)
            {
                int level = data.definition.level;
               
                if (cached.TryGetValue(level, out var list))
                {
                    list.Add(data);        
                }
                else
                {
                    cached.Add(level, new List<SpawnConfigData> { data });
                }
            }
        }

        public override void OnImported()
        {
            for (int i = 0; i < spawns.Count; i++)
            {
                SpawnConfigData spawn = spawns[i];
                spawn.Parse();
                spawns[i] = spawn;
            }
        }

        public override void OnCompleteImported()
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
                
                if (monsterConfig.TryGetMonster(monsterId, out _)) continue;

                Debug.LogError($"Not found monster '{monsterId}', json '{JsonUtility.ToJson(spawn)}'");
            }
#endif
        }

        public bool TryGetSpawn(int level, out List<SpawnConfigData> data)
        {
            return cached.TryGetValue(level, out data);
        }
    }
}
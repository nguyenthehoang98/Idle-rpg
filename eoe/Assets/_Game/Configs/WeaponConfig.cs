using System;
using System.Collections.Generic;
using _KITSystem.Config;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _Game.Configs
{
    [Serializable]
    public class WeaponConfig : IGameConfig
    {
        [SerializeField] private List<WeaponData> weapons = new List<WeaponData>();

        private Dictionary<int, WeaponData> cached;
        
        public void OnMappingValue()
        {
            cached = new Dictionary<int, WeaponData>();

            foreach (var data in weapons)
            {
                if (!cached.TryAdd(data.id, data)) Debug.LogError($"Duplicate weapon '{data.id}'");
            }
        }

        public void OnPostImported()
        {
        }

        public void OnValidateLinkConfig()
        {
#if UNITY_EDITOR
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_BattleSource/Configs/SkillConfig.json");
            SkillConfig skillConfig = JsonUtility.FromJson<SkillConfig>(asset.text);
            skillConfig.OnMappingValue();

            for (int i = 0; i < weapons.Count; i++)
            {
                WeaponData weapon = weapons[i];
                if (skillConfig.TryGetSkillOnEditor(weapon.skillId, out var skillData))
                {
                    weapon.skillData = skillData;
                }
                else Debug.LogError($"Not found skill '{weapon.skillId}' at weapon '{weapon.id}'");
                weapons[i] = weapon;
            }
#endif
        }

        public bool TryGetWeaponData(int weaponId, out WeaponData weaponData)
        {
            return cached.TryGetValue(weaponId, out weaponData);
        }
    }
    
    [Serializable]
    public struct WeaponData
    {
        public int id;
        public string prefabName;
        public int skillId;
        public float cooldown;
        public float attackSpeed;
        public SkillData skillData;
        public string attackAudioClip;
        public float attackVolume;
    }
}
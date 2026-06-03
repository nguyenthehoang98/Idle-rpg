using System;
using System.Collections.Generic;
using _KITSystem.Data;
using UnityEngine;

namespace _FightCode.Config
{
    [Serializable]
    public class MonsterConfig : IGameConfig
    {
        [SerializeField] private List<MonsterData> monsters = new List<MonsterData>();
    
        private Dictionary<int, MonsterData> byId;
    
        public void OnMappingValue()
        {
            byId = new Dictionary<int, MonsterData>();

            foreach (var m in monsters)
            {
                byId.Add(m.ID, m);
            }
        }
        
        public void OnPostImported()
        {
            foreach (var data in monsters)
            {
                if (data.AttackRange.Length != 2)
                    Debug.LogError(
                        $"EquipmentUpgradeConfig: AttackRange Length '{data.AttackRange.Length}'. Id '{data.ID}'"
                    );
                if (data.ColliderOffset.Length != 2)
                    Debug.LogError(
                        $"EquipmentUpgradeConfig: ColliderOffset Length '{data.ColliderOffset.Length}'. Id '{data.ID}'"
                    );
                if (data.HealthBarOffset.Length != 2)
                    Debug.LogError(
                        $"EquipmentUpgradeConfig: HpBarOffset Length '{data.HealthBarOffset.Length}'. Id '{data.ID}'"
                    );
                if (data.FloatingTextOffset.Length != 2)
                    Debug.LogError(
                        $"EquipmentUpgradeConfig: FloatingTextOffset Length '{data.FloatingTextOffset.Length}'. Id '{data.ID}'"
                    );
            }
        }

        public bool Find(int monsterId, out MonsterData skill)
        {
            return byId.TryGetValue(monsterId, out skill);
        }
    }

    [Serializable]
    public struct MonsterData
    {
        public int ID;
        public string NameKey;
        public string PrefabName;
        public int ClassID;
        public float StopDistance;
        public float[] AttackRange;
        public int ActiveSkill;         // kĩ năng active
        public int PassiveSkill;        // kĩ năng passive
        public float Scale;
        public float ColliderRadius;
        public float[] ColliderOffset;
        public float[] HealthBarOffset;
        public float[] FloatingTextOffset;
        public float KnockbackResistance;
        public float KnockbackResistanceCD;
        public float StunResistance;
        public float StunResistanceCD;
        public string DeathSfx;
    }
}
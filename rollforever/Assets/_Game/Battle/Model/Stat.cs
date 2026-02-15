using System;
using _Game.Battle.Data;
using Unity.Collections;
using UnityEngine;

namespace _Game.Battle
{
    [Serializable]
    public struct Stat
    {
        [SerializeField] StatType type;
        [SerializeField] float baseValue;
        [SerializeField] float value;
        [SerializeField] NativeList<StatModifier> modifiers;
        
        public Stat(StatType type, float baseValue)
        {
            this.type = type;
            this.baseValue = value = baseValue;
            this.modifiers = new NativeList<StatModifier>(Allocator.Persistent);
        }

        public float Value => value;

        public void AddModifier(StatModifier modifier)
        {
            modifiers.Add(modifier);
            Recalculate();
        }
        
        public void RemoveModifier(StatModifier modifier)
        {
            for (int i = 0; i < modifiers.Length; i++)
            {
                if (modifiers[i].id == modifier.id)
                {
                    modifiers.RemoveAtSwapBack(i);
                    Recalculate();
                    return;
                }
            }
        }
        
        void Recalculate()
        {
            float flat = 0;
            float percent = 0;

            foreach (var m in modifiers)
            {
                if (m.type == StatModifierType.Additive)
                    flat += m.value;
                else if (m.type == StatModifierType.Multiplicative)
                    percent += m.value;
            }

            value = (baseValue + flat) * (1f + percent);
        }
    }

    public enum StatType
    {
        Attack, Defense, Health,
        SkillReduceCooldown, 
        CriticalRate, CriticalDamage,
        MoveSpeed,
    }
}
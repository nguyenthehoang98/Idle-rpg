using System;
using Unity.Collections;
using UnityEngine;

namespace _Game.Battle.Model
{
    [Serializable]
    public struct Stat
    {
        [SerializeField] float baseValue;
        [SerializeField] float value;
        [SerializeField] NativeList<StatModifier> modifiers;
        
        public Stat(float baseValue)
        {
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
        Attack, // chỉ số tấn công 
        Defense, // chỉ số phòng thủ
        MaxHealth, // chỉ số máu
        CriticalRate, // chỉ số khả năng gây chí mạng
        CriticalDamage, // chỉ số tăng sát thương khi chi mạng
        MoveSpeed, // tốc độ di chuyển của monster
        AttackPercent, // hệ số tăng thêm chỉ số tấn công (nhân với atk cuối)
        MaxHealthPercent, // hệ số tăng thêm chỉ số máu (nhân với maxHealth cuối)
        DamageAreaPercent, // phạm vi gây sát thương
        Bounce, // số lần nảy
        EffectDuration, // thời gian hiệu ứng
        EffectDamagePercent, // tỉ lệ sát thương với 1 số sát thương nhất định: độc, cháy...
    }
}
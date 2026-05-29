using System;
using System.Collections.Generic;

namespace _KITSystem.Formula
{
    [Serializable]
    internal sealed class StatValue
    {
        private readonly List<StatModifier> modifiers = new List<StatModifier>();
        private bool dirty = true;
        private float cachedValue;

        public float BaseValue { get; private set; }
        
        public IReadOnlyList<StatModifier> Modifiers => modifiers;

        public StatValue(float baseValue = 0)
        {
            BaseValue = baseValue;
        }

        public void SetBase(float value)
        {
            if (Math.Abs(BaseValue - value) < float.Epsilon) return;
            BaseValue = value;
            dirty = true;
        }

        public void AddBase(float value)
        {
            if (Math.Abs(value) < float.Epsilon) return;
            BaseValue += value;
            dirty = true;
        }

        public void AddModifier(StatModifier modifier)
        {
            modifiers.Add(modifier);
            modifiers.Sort((a, b) => a.Order.CompareTo(b.Order));
            dirty = true;
        }

        public bool RemoveModifier(StatModifier modifier)
        {
            bool removed = modifiers.Remove(modifier);
            dirty |= removed;
            return removed;
        }

        public int RemoveModifiersFrom(object source)
        {
            if (source == null) return 0;

            int removed = modifiers.RemoveAll(m => ReferenceEquals(m.Source, source));
            dirty |= removed > 0;
            return removed;
        }

        public void ClearModifiers()
        {
            if (modifiers.Count == 0) return;
            modifiers.Clear();
            dirty = true;
        }

        public float GetValue()
        {
            if (!dirty) return cachedValue;

            float flat = 0;
            float addPercent = 0;
            float moreMultiplier = 1f;

            for (int i = 0; i < modifiers.Count; i++)
            {
                StatModifier modifier = modifiers[i];
                switch (modifier.ModifierType)
                {
                    case StatModifierType.Flat:
                        flat += modifier.Value;
                        break;
                    case StatModifierType.AddPercent:
                        addPercent += modifier.Value;
                        break;
                    case StatModifierType.MorePercent:
                        moreMultiplier *= 1f + modifier.Value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            cachedValue = (BaseValue + flat) * (1f + addPercent) * moreMultiplier;
            dirty = false;
            return cachedValue;
        }
    }
}
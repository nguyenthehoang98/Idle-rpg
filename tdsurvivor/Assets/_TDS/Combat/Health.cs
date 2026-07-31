using System;
using UnityEngine;

namespace _TDS.Combat
{
    /// <summary>
    /// Health component cho entity (Monster, Hero, BaseCore).
    /// Thay thế HealthData ECS-lite của Recovery 2 bằng component đơn giản
    /// phù hợp kiến trúc MonoBehaviour hiện tại của _TDS.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;

        private int currentHealth;

        public event Action<int, int> OnHpChanged;  // (current, max)
        public event Action OnDied;

        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;
        public bool IsAlive => currentHealth > 0;
        public float HealthPercent => maxHealth <= 0 ? 0f : (float)currentHealth / maxHealth;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void Initialize(int maxHp)
        {
            maxHealth = Mathf.Max(1, maxHp);
            currentHealth = maxHealth;
            OnHpChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(int damage)
        {
            if (!IsAlive) return;

            currentHealth = Mathf.Max(0, currentHealth - Mathf.Max(0, damage));

            OnHpChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                OnDied?.Invoke();
            }
        }

        public void Heal(int amount)
        {
            if (!IsAlive) return;

            currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.Max(0, amount));

            OnHpChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}

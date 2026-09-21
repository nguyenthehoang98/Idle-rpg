using System;
using UnityEngine;

namespace _TDS.Battle
{
    /// <summary>
    /// HP chung của người chơi. Hero chỉ tìm mục tiêu & tấn công; mọi sát thương
    /// nhắm vào hero đều dồn vào pool này, hết máu = thua.
    /// MaxHealth = tổng MaxHealth của các hero đang sống (hero config là nguồn stat duy nhất).
    /// </summary>
    public static class PlayerVitals
    {
        /// <summary>Hết máu. GameplayScene lắng nghe để xử lý thua.</summary>
        public static event Action OnDied;

        private static int maxHealth;
        private static int currentHealth;
        private static bool died;

        public static int MaxHealth => maxHealth;
        public static int CurrentHealth => currentHealth;
        public static bool IsDead => died;

        /// <summary>Bắt đầu run mới: pool rỗng, hero spawn sau đó tự cộng vào.</summary>
        public static void Reset()
        {
            maxHealth = 0;
            currentHealth = 0;
            died = false;
        }

        /// <summary>Tính lại pool theo hero sống. Gọi khi hero spawn hoặc đổi MaxHealth.</summary>
        public static void SyncMaxHealth()
        {
            int newMax = 0;
            foreach (Hero hero in Hero.AliveHeroes)
            {
                if (hero != null) newMax += hero.MaxHealth;
            }

            newMax = Mathf.Max(0, newMax);
            if (newMax == maxHealth) return;

            // tăng máu tối đa (hero mới/upgrade) thì cộng luôn vào máu hiện tại, giữ hành vi cũ
            currentHealth = Mathf.Clamp(currentHealth + newMax - maxHealth, 0, newMax);
            maxHealth = newMax;
        }

        public static int TakeDamage(int damage)
        {
            // chưa có hero nào => chưa có pool, không nhận sát thương
            if (damage <= 0 || died || maxHealth <= 0) return 0;

            int dealt = Mathf.Min(damage, currentHealth);
            currentHealth -= dealt;

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                died = true;
                OnDied?.Invoke();
            }

            return dealt;
        }

        public static void Heal(int amount)
        {
            if (amount <= 0 || died) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        }
    }
}

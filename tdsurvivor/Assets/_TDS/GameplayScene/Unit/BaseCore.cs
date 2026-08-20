using System;
using _TDS.Core;
using UnityEngine;

namespace _TDS.GameplayScene.Unit
{
    public class BaseCore : MonoBehaviour
    {
        public static BaseCore Instance { get; private set; }

        [SerializeField] private int maxHp = 100;

        public int MaxHp => maxHp;
        public int CurrentHp { get; private set; }
        public bool IsAlive => CurrentHp > 0;

        public event Action<int, int> OnHpChanged; // (currentHp, maxHp)
        public event Action OnDied;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            CurrentHp = maxHp;
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive) return;
            if (amount <= 0) return;

            CurrentHp -= amount;
            if (CurrentHp < 0) CurrentHp = 0;

            OnHpChanged?.Invoke(CurrentHp, maxHp);

            if (CurrentHp <= 0)
            {
                OnDied?.Invoke();
                GameManager.Instance?.SetState(GameState.GameOver);
            }
        }

        public void Heal(int amount)
        {
            if (!IsAlive) return;
            if (amount <= 0) return;

            CurrentHp = Mathf.Min(CurrentHp + amount, maxHp);
            OnHpChanged?.Invoke(CurrentHp, maxHp);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}

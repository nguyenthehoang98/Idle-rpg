using System;
using System.Collections.Generic;
using _GameToolkit.Colliders;
using _GameToolkit.Skills;
using _GameToolkit.ResourceManagement;
using _GameToolkit.Share;
using _TDS.GameConfig;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace _TDS.Battle
{
    [RequireComponent(typeof(MonsterSortingLayer))]
    public class Monster : Unique
    {
        [SerializeField] private Transform scale;
        [SerializeField] private UnityEvent OnBeHit;
        [SerializeField] private UnityEvent OnDeath;
        [SerializeField] private GameObject deathVfx;

        public static event Action<Monster> OnMonsterEnable;
        public static event Action<Monster> OnMonsterDisable;

        private static HashSet<int> deathVfxInPool = new HashSet<int>();

        private Vector3 targetPosition;
        private Vector3 previousPosition;
        private float elapsedTime;
        private float deltaTime;
        private readonly Dictionary<ModifierSkillAction, SkillModifierData> activeModifiers =
            new Dictionary<ModifierSkillAction, SkillModifierData>();

        public bool IsStunned => HasModifier(SkillModifierType.Stun);
        public bool IsSilenced => HasModifier(SkillModifierType.Silence);

        public float MoveSpeedMultiplier
        {
            get
            {
                float multiplier = 1f;
                foreach (SkillModifierData modifier in activeModifiers.Values)
                {
                    if (modifier.type != SkillModifierType.Slow) continue;
                    multiplier = Mathf.Min(multiplier, 1f - Mathf.Clamp01(modifier.value));
                }

                return multiplier;
            }
        }

        public float Radius
        {
            get { return GetComponent<CircleCollider2D>().radius; }
        }

        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public int Attack { get; private set; }

        public float AttackRange { get; private set; }
        public float DamageCooldown { get; private set; }
        public float AttackTimer { get; set; }

        public void SetCombatData(int maxHealth, int attack, float attackRange = 1f, float damageCooldown = 1f)
        {
            MaxHealth = CurrentHealth = maxHealth;
            Attack = attack;
            AttackRange = attackRange;
            DamageCooldown = Mathf.Max(0f, damageCooldown);
            AttackTimer = 0f;
        }

        public int TakeDamage(int damage)
        {
            if (damage <= 0 || CurrentHealth <= 0) return 0;

            int dealt = Mathf.Min(damage, CurrentHealth);
            CurrentHealth -= dealt;

            if (CurrentHealth <= 0) Death();
            return dealt;
        }

        public void ApplyModifier(ModifierSkillAction action, SkillModifierData modifier)
        {
            if (action == null) return;
            activeModifiers[action] = modifier;
        }

        public void RemoveModifier(ModifierSkillAction action)
        {
            if (action != null) activeModifiers.Remove(action);
        }

        private bool HasModifier(SkillModifierType type)
        {
            foreach (SkillModifierData modifier in activeModifiers.Values)
            {
                if (modifier.type == type) return true;
            }

            return false;
        }

        private bool isInitialized;

        private void Awake()
        {
            if (deathVfx != null && deathVfxInPool.Add(deathVfx.GetHashCode()))
            {
                Pool.RegisterPool(deathVfx.gameObject, true);
            }
        }

        private void Update()
        {
            if (!isInitialized) return;

            elapsedTime += Time.fixedDeltaTime;

            float t = Mathf.Clamp01(elapsedTime / deltaTime);

            transform.position = Vector3.Lerp(previousPosition, targetPosition, t);
        }

        public void Initialize(SpawnScaleDefinition scaleDefinition)
        {
            scale.localScale = scaleDefinition.sizeMultiplier * Vector3.one;

            OnMonsterEnable?.Invoke(this);

            gameObject.SetActive(true);

            isInitialized = true;
        }

        public void SetPosition(Vector3 position, float dt)
        {
            previousPosition = transform.position;

            targetPosition = position;

            deltaTime = dt;

            elapsedTime = 0;
        }

        public void BeHit() => OnBeHit?.Invoke();

        public void Death()
        {
            if (!isInitialized) return;

            OnDeath?.Invoke();

            Debug.Log($"[Monster:{name}] Death");

            if (deathVfx != null)
            {
                GameObject ps = Pool.Instantiate(deathVfx.gameObject);
                ps.transform.position = transform.position;
                ps.transform.rotation = Quaternion.identity;
            }

            OnMonsterDisable?.Invoke(this);

            isInitialized = false;

            foreach (ModifierSkillAction action in activeModifiers.Keys)
            {
                action.Interrupt();
            }

            activeModifiers.Clear();
            OnDeath?.Invoke();

            Destroy();
        }

        private async void Destroy()
        {
            await UniTask.NextFrame(PlayerLoopTiming.Update);

            Pool.Destroy(gameObject);
        }

        public override int Id()
        {
            return GetHashCode();
        }
    }
}
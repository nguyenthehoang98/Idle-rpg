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
        public static event Action<Monster, int, int> OnMonsterRewarded;

        public event Action<SkillModifierType> OnModifierApplied;
        public event Action<SkillModifierType> OnModifierRemoved;

        private static HashSet<int> deathVfxInPool = new HashSet<int>();

        private Vector3 targetPosition;
        private Vector3 previousPosition;
        private float elapsedTime;
        private float deltaTime;
        private readonly Dictionary<ModifierSkillAction, SkillModifierData> activeModifiers =
            new Dictionary<ModifierSkillAction, SkillModifierData>();
        private SpriteRenderer[] statusRenderers;
        private Color[] baseRendererColors;
        private int nextSkillIndex;
        private float skillTimer;

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
        public int ExperienceReward { get; private set; }
        public int GoldReward { get; private set; }
        public MonsterRank Rank { get; private set; }
        public IReadOnlyList<MonsterSkillConfigData> Skills { get; private set; }

        public float AttackRange { get; private set; }
        public float DamageCooldown { get; private set; }
        public float AttackTimer { get; set; }

        public void SetCombatData(
            int maxHealth,
            int attack,
            float attackRange = 1f,
            float damageCooldown = 1f,
            int experienceReward = 0,
            int goldReward = 0,
            MonsterRank rank = MonsterRank.Normal,
            MonsterSkillConfigData[] skills = null)
        {
            MaxHealth = CurrentHealth = maxHealth;
            Attack = attack;
            AttackRange = attackRange;
            DamageCooldown = Mathf.Max(0f, damageCooldown);
            ExperienceReward = Mathf.Max(0, experienceReward);
            GoldReward = Mathf.Max(0, goldReward);
            Rank = rank;
            Skills = skills ?? Array.Empty<MonsterSkillConfigData>();
            nextSkillIndex = 0;
            skillTimer = 0f;
            AttackTimer = 0f;
        }

        public bool TryGetReadySkill(float deltaTime, out MonsterSkillConfigData skill)
        {
            skill = default;
            if (Rank == MonsterRank.Normal || Skills == null || Skills.Count == 0)
            {
                return false;
            }

            skillTimer -= Mathf.Max(0f, deltaTime);
            if (skillTimer > 0f)
            {
                return false;
            }

            skill = Skills[nextSkillIndex % Skills.Count];
            return skill.skillId > 0;
        }

        public void CommitSkill(MonsterSkillConfigData skill)
        {
            if (Skills == null || Skills.Count == 0)
            {
                return;
            }

            nextSkillIndex = (nextSkillIndex + 1) % Skills.Count;
            skillTimer = Mathf.Max(0.1f, skill.cooldown);
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
            RefreshModifierVisual();
            OnModifierApplied?.Invoke(modifier.type);
        }

        public void RemoveModifier(ModifierSkillAction action)
        {
            if (action == null || !activeModifiers.TryGetValue(action, out SkillModifierData modifier))
            {
                return;
            }

            activeModifiers.Remove(action);
            RefreshModifierVisual();
            OnModifierRemoved?.Invoke(modifier.type);
        }

        private bool HasModifier(SkillModifierType type)
        {
            foreach (SkillModifierData modifier in activeModifiers.Values)
            {
                if (modifier.type == type) return true;
            }

            return false;
        }

        private void RefreshModifierVisual()
        {
            if (statusRenderers == null || statusRenderers.Length == 0)
            {
                CacheStatusRenderers();
            }

            if (statusRenderers.Length == 0)
            {
                return;
            }

            Color tint = Color.white;
            if (IsStunned)
            {
                tint = new Color(1f, 0.45f, 0.1f, 1f);
            }
            else if (HasModifier(SkillModifierType.Bleed))
            {
                tint = new Color(1f, 0.35f, 0.45f, 1f);
            }
            else if (IsSilenced)
            {
                tint = new Color(0.75f, 0.45f, 1f, 1f);
            }
            else if (HasModifier(SkillModifierType.Slow))
            {
                tint = new Color(1f, 0.8f, 0.25f, 1f);
            }

            for (int i = 0; i < statusRenderers.Length; i++)
            {
                Color baseColor = baseRendererColors[i];
                statusRenderers[i].color = new Color(
                    baseColor.r * tint.r,
                    baseColor.g * tint.g,
                    baseColor.b * tint.b,
                    baseColor.a);
            }
        }

        private void CacheStatusRenderers()
        {
            statusRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            baseRendererColors = new Color[statusRenderers.Length];
            for (int i = 0; i < statusRenderers.Length; i++)
            {
                baseRendererColors[i] = statusRenderers[i].color;
            }
        }

        private bool isInitialized;
        private bool rewardGranted;

        private void Awake()
        {
            CacheStatusRenderers();

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
            rewardGranted = false;
            scale.localScale = scaleDefinition.sizeMultiplier * Vector3.one;
            RefreshModifierVisual();

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
            GrantRewardOnce();

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
            RefreshModifierVisual();

            Destroy();
        }

        private void GrantRewardOnce()
        {
            if (rewardGranted)
            {
                return;
            }

            rewardGranted = true;
            OnMonsterRewarded?.Invoke(this, ExperienceReward, GoldReward);
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
using System;
using System.Collections;
using System.Collections.Generic;
using _GameToolkit.ResourceManagement;
using _GameToolkit.Skills;
using _GameToolkit.Share;
using _GameToolkit.Statistics;
using _TDS.GameConfig;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using _TDS.Utils;

namespace _TDS.Battle
{
    public class Hero : Unique
    {
        private Dictionary<StatId, Stat> stats;
        private readonly HashSet<Monster> monsters = new HashSet<Monster>();
        private Coroutine attackCoroutine;
        private Monster pendingAttackTarget;
        private SkillConfigData pendingAttackSkill;
        private bool hasPendingAttackSkill;
        private readonly object powerUpgradeSource = new object();
        private IReadOnlyList<StatUpgradeConfigData> powerUpgrades = Array.Empty<StatUpgradeConfigData>();

        [Tooltip("Transform làm gốc spawn projectile.")]
        [SerializeField] private Transform muzzle;
        [Tooltip("Pivot hình ảnh để flip mặt theo phía của mục tiêu.")]
        [SerializeField] private Transform rootPivot;
        [Header("Attack Hit Feedback")]
        [SerializeField] private GameObject hitFxPrefab;
        [SerializeField] private AudioClip hitAudio;
        [Header("Power Feedback")]
        [Tooltip("Optional prefab. If empty, Hero creates a simple fire ring at runtime.")]
        [SerializeField] private GameObject powerEffectPrefab;
        [Tooltip("Được gọi một lần mỗi đòn đánh, sau khi hero đã tìm thấy mục tiêu.")]
        [SerializeField] private UnityEvent onAttack = new UnityEvent();

        private GameObject powerEffectInstance;
        private ParticleSystem[] powerEffectParticles;

        private static readonly HashSet<string> hitFxPoolNames = new HashSet<string>();

        private void Awake()
        {
            if (hitFxPrefab != null && hitFxPoolNames.Add(hitFxPrefab.name))
            {
                Pool.RegisterPool(hitFxPrefab, true);
            }
        }

        private void OnDestroy()
        {
            if (hitFxPrefab != null && hitFxPoolNames.Remove(hitFxPrefab.name))
            {
                Pool.UnRegisterPool(hitFxPrefab);
            }
        }

        public Transform Muzzle => muzzle;
        public UnityEvent OnAttack => onAttack;

        protected int AttackId { get; private set; }
        protected SkillConfigData SkillConfig { get; private set; }

        /// <summary>Hero sống trên sân (đăng ký khi enable, gỡ khi disable/dead).</summary>
        public static event Action<Hero> OnHeroEnable;
        public static event Action<Hero> OnHeroDisable;
        private static readonly HashSet<Hero> aliveHeroes = new HashSet<Hero>();
        public static IReadOnlyCollection<Hero> AliveHeroes => aliveHeroes;
        /// <summary>Hero chết (hp <= 0). UI/gameplay lắng nghe để xử lý thua.</summary>
        public event Action OnDied;
        public event Action<Hero> OnOverdriveStarted;
        public event Action<Hero> OnOverdriveEnded;

        public int HeroId { get; private set; }
        public int CircuitSlotIndex { get; private set; } = -1;
        public int SkillId => AttackId;
        public bool IsOverdriveActive { get; private set; }
        public float PowerDuration { get; private set; }
        public float OverdriveRemaining { get; private set; }
        public int OverdriveActivations { get; private set; }

        public int CurrentHealth { get; private set; }
        public int MaxHealth { get; private set; }
        public int TotalDamageDealt { get; private set; }
        public bool IsDead => CurrentHealth <= 0;

        public Stat GetStat(StatId id)
        {
            return stats != null && stats.TryGetValue(id, out Stat stat) ? stat : null;
        }

        public void SetCircuitSlotIndex(int slotIndex)
        {
            CircuitSlotIndex = slotIndex;
        }

        private void OnEnable()
        {
            Monster.OnMonsterEnable += AddMonster;
            Monster.OnMonsterDisable += RemoveMonster;
            aliveHeroes.Add(this);
            OnHeroEnable?.Invoke(this);
        }

        private void OnDisable()
        {
            Monster.OnMonsterEnable -= AddMonster;
            Monster.OnMonsterDisable -= RemoveMonster;
            aliveHeroes.Remove(this);
            OnHeroDisable?.Invoke(this);

            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }

            pendingAttackTarget = null;
            hasPendingAttackSkill = false;
            RemovePowerUpgrade();
            StopPowerEffect();
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || IsDead) return;

            int healthBefore = CurrentHealth;
            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);

            Debug.Log($"[Hero:{name}] TakeDamage({healthBefore - CurrentHealth}) hp {healthBefore} -> {CurrentHealth}");

            if (CurrentHealth <= 0)
            {
                aliveHeroes.Remove(this);
                Debug.Log($"[Hero:{name}] DIE");

                if (attackCoroutine != null)
                {
                    StopCoroutine(attackCoroutine);
                    attackCoroutine = null;
                }

                pendingAttackTarget = null;
                hasPendingAttackSkill = false;
                RemovePowerUpgrade();
                StopPowerEffect();
                OnDied?.Invoke();
                OnHeroDisable?.Invoke(this);
            }
        }

        private void AddMonster(Monster monster) => monsters.Add(monster);

        private void RemoveMonster(Monster monster) => monsters.Remove(monster);

        public void Initialize(HeroConfigData heroConfigData, SkillConfigData skillConfigData)
        {
            RemovePowerUpgrade();
            powerUpgrades = Array.Empty<StatUpgradeConfigData>();
            HeroId = heroConfigData.id;
            SkillConfig = skillConfigData;
            IsOverdriveActive = false;
            OverdriveRemaining = 0f;
            OverdriveActivations = 0;
            PowerDuration = heroConfigData.powerDuration > 0f
                ? heroConfigData.powerDuration
                : EnergyCircuit.DefaultOverdriveDuration;

            MaxHealth = Mathf.Max(1, heroConfigData.health);
            CurrentHealth = MaxHealth;
            TotalDamageDealt = 0;

            stats = new Dictionary<StatId, Stat>
            {
                { StatId.MaxHealth, new Stat(MaxHealth) },
                { StatId.Attack, new Stat(heroConfigData.attack) },
                { StatId.AttackRange, new Stat(heroConfigData.attackRange) },
                { StatId.AttackSpeed, new Stat(heroConfigData.attackSpeed) },
                { StatId.CritChance, new Stat(heroConfigData.critChance) },
                { StatId.CritDamage, new Stat(heroConfigData.critDamage) },
                { StatId.Lifesteal, new Stat(heroConfigData.lifesteal) },
                { StatId.SkillCooldown, new Stat(heroConfigData.skillCooldown) },
                { StatId.ExpMultiplier, new Stat(heroConfigData.expMultiplier) },
            };

            AttackId = heroConfigData.attackId;

            if (attackCoroutine == null && Application.isPlaying)
            {
                attackCoroutine = StartCoroutine(AutoAttackEnumerator());
            }
        }

        public void SetPowerUpgrades(IReadOnlyList<StatUpgradeConfigData> upgrades)
        {
            RemovePowerUpgrade();
            powerUpgrades = upgrades ?? Array.Empty<StatUpgradeConfigData>();
        }

        private void ApplyPowerUpgrade()
        {
            foreach (StatUpgradeConfigData upgrade in powerUpgrades)
            {
                if (upgrade.stat == StatId.None || Mathf.Approximately(upgrade.value, 0f))
                {
                    continue;
                }

                Stat stat = GetStat(upgrade.stat);
                if (stat == null) continue;

                stat.AddModifier(new StatModifier(
                    upgrade.value,
                    upgrade.percent ? StatModType.PercentAdd : StatModType.Flat,
                    powerUpgradeSource));
            }

            RefreshMaxHealth();
        }

        private void RemovePowerUpgrade()
        {
            if (stats == null) return;

            foreach (Stat stat in stats.Values)
            {
                stat.RemoveAllModifiersFromSource(powerUpgradeSource);
            }

            RefreshMaxHealth();
        }

        private void RefreshMaxHealth()
        {
            Stat maxHealth = GetStat(StatId.MaxHealth);
            if (maxHealth == null) return;

            int previousMaxHealth = MaxHealth;
            MaxHealth = Mathf.Max(1, Mathf.RoundToInt(maxHealth.Value));
            CurrentHealth = Mathf.Clamp(
                CurrentHealth + MaxHealth - previousMaxHealth,
                0,
                MaxHealth);
        }

        public bool ApplyHeroUpgrade(StatId statId, float value, bool percent)
        {
            Stat stat = GetStat(statId);
            if (stat == null) return false;

            float previousMaxHealth = MaxHealth;
            stat.AddModifier(new StatModifier(
                value,
                percent ? StatModType.PercentAdd : StatModType.Flat,
                this));

            if (statId == StatId.MaxHealth)
            {
                MaxHealth = Mathf.Max(1, Mathf.RoundToInt(stat.Value));
                CurrentHealth = Mathf.Clamp(
                    CurrentHealth + Mathf.RoundToInt(MaxHealth - previousMaxHealth),
                    0,
                    MaxHealth);
            }

            return true;
        }

        public bool ApplySkillUpgrade(SkillUpgradeStat statId, float value, bool percent)
        {
            SkillConfigData skill = SkillConfig;
            float Apply(float current)
            {
                return percent ? current * (1f + value) : current + value;
            }

            switch (statId)
            {
                case SkillUpgradeStat.ProjectileSpeed:
                    skill.projectileSpeed = Mathf.Max(0f, Apply(skill.projectileSpeed));
                    break;
                case SkillUpgradeStat.ProjectileDuration:
                    skill.projectileDuration = Mathf.Max(0.01f, Apply(skill.projectileDuration));
                    break;
                case SkillUpgradeStat.HitCount:
                    skill.hitCount = Mathf.Max(1, Mathf.RoundToInt(Apply(skill.hitCount)));
                    break;
                case SkillUpgradeStat.HitInterval:
                    skill.hitInterval = Mathf.Max(0f, Apply(skill.hitInterval));
                    break;
                case SkillUpgradeStat.CollisionDuration:
                    skill.collisionDuration = Mathf.Max(0f, Apply(skill.collisionDuration));
                    break;
                case SkillUpgradeStat.CollisionDelayInit:
                    skill.collisionDelayInit = Mathf.Max(0f, Apply(skill.collisionDelayInit));
                    break;
                case SkillUpgradeStat.SizeMultiplier:
                    skill.sizeMultiplier = Mathf.Max(0f, Apply(skill.sizeMultiplier));
                    break;
                case SkillUpgradeStat.SpreadDamageScale:
                    skill.spreadDamageScale = Mathf.Max(0f, Apply(skill.spreadDamageScale));
                    break;
                case SkillUpgradeStat.ParallelDamageScale:
                    skill.parallelDamageScale = Mathf.Max(0f, Apply(skill.parallelDamageScale));
                    break;
                case SkillUpgradeStat.ProjectileDistanceStep:
                    skill.projectileDistanceStep = Mathf.Max(0f, Apply(skill.projectileDistanceStep));
                    break;
                case SkillUpgradeStat.ProjectileAngleStep:
                    skill.projectileAngleStep = Mathf.Max(0f, Apply(skill.projectileAngleStep));
                    break;
                case SkillUpgradeStat.SpreadProjectileCount:
                    skill.spreadProjectileCount = Mathf.Max(0, Mathf.RoundToInt(Apply(skill.spreadProjectileCount)));
                    break;
                case SkillUpgradeStat.ParallelProjectileCount:
                    skill.parallelProjectileCount = Mathf.Max(0, Mathf.RoundToInt(Apply(skill.parallelProjectileCount)));
                    break;
                case SkillUpgradeStat.ExplosiveRadius:
                    skill.explosiveRadius = Mathf.Max(0f, Apply(skill.explosiveRadius));
                    break;
                case SkillUpgradeStat.ExplosiveDamageScale:
                    skill.explosiveDamageScale = Mathf.Max(0f, Apply(skill.explosiveDamageScale));
                    break;
                case SkillUpgradeStat.InstantKillTargetBelowHealthPercent:
                    skill.instantKillTargetBelowHealthPercent = Mathf.Clamp01(Apply(skill.instantKillTargetBelowHealthPercent));
                    break;
                case SkillUpgradeStat.DamageTickInterval:
                    skill.damageTickInterval = Mathf.Max(0f, Apply(skill.damageTickInterval));
                    break;
                default:
                    return false;
            }

            SkillConfig = skill;
            return true;
        }

        // Loop tìm & đánh theo step, chạy bằng Coroutine thay vì Update
        protected virtual IEnumerator AutoAttackEnumerator()
        {
            Debug.Log("Start AutoAttackEnumerator");
            while (true)
            {
                float interval = Mathf.Max(0.01f, 1f / GetStat(StatId.AttackSpeed).Value);
                yield return new WaitForSeconds(interval);

                Monster target = FindTarget(SkillConfig.findTarget);
                if (target == null) continue;

                CastSkill(target);
            }
        }

        private Monster FindTarget(TargetSelectionType selectionType)
        {
            float sqrRange = GetStat(StatId.AttackRange).Value;
            sqrRange *= sqrRange;

            Vector3 position = transform.position;
            Monster best = null;
            float bestScore = float.MinValue; // score càng cao càng tốt

            foreach (Monster monster in monsters)
            {
                if (monster == null || !monster.isActiveAndEnabled) continue;

                float sqrDistance = (monster.transform.position - position).sqrMagnitude;

                if (sqrDistance > sqrRange) continue; // ngoài tầm đánh

                // đảo dấu nhóm muốn "càng nhỏ càng tốt" để so theo max thống nhất
                float score;

                switch (selectionType)
                {
                    case TargetSelectionType.Farthest: score = sqrDistance; break;
                    case TargetSelectionType.HpLowest: score = -monster.CurrentHealth; break;
                    case TargetSelectionType.HpHighest: score = monster.CurrentHealth; break;
                    case TargetSelectionType.AtkLowest: score = -monster.Attack; break;
                    case TargetSelectionType.AtkHighest: score = monster.Attack; break;
                    default: score = -sqrDistance; break; // Nearest, None: gần nhất
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    best = monster;
                }
            }

            return best;
        }

        protected virtual void CastSkill(Monster target)
        {
            CastSkill(target, SkillConfig);
        }

        private void CastSkill(Monster target, SkillConfigData skill)
        {
            if (target == null) return;

            FaceTarget(target);
            pendingAttackTarget = target;
            pendingAttackSkill = skill;
            hasPendingAttackSkill = true;
            onAttack?.Invoke();
        }

        private void FaceTarget(Monster target)
        {
            if (rootPivot == null) return;

            float deltaX = target.transform.position.x - rootPivot.position.x;
            if (Mathf.Abs(deltaX) < 0.001f) return;

            Vector3 scale = rootPivot.localScale;
            scale.x = Mathf.Abs(scale.x) * (deltaX < 0f ? 1f : -1f);
            rootPivot.localScale = scale;
        }

        /// <summary>
        /// Spawns the pending attack skill. Call this from the delayed LitMotion attack animation event.
        /// </summary>
        public void SpawnSkill()
        {
            Monster target = pendingAttackTarget;
            pendingAttackTarget = null;
            SkillConfigData skill = hasPendingAttackSkill ? pendingAttackSkill : SkillConfig;
            hasPendingAttackSkill = false;

            if (target == null || !target.isActiveAndEnabled || target.CurrentHealth <= 0)
                return;

            if (muzzle == null)
            {
                Debug.LogError($"[Hero:{name}] Cannot spawn skill because Muzzle is not assigned.");
                return;
            }

            CastSkillAtTarget(target, skill);
        }

        private void PlayHitFeedback(Vector3 position)
        {
            if (hitFxPrefab != null)
            {
                GameObject fx = Pool.Instantiate(hitFxPrefab, position, true);
                fx.transform.rotation = Quaternion.identity;
            }

            if (hitAudio != null)
            {
                SoundUtils.Instance?.PlayOneShot(hitAudio, interval: 0f);
            }
        }

        private void StartPowerEffect()
        {
            if (!Application.isPlaying) return;

            if (powerEffectInstance == null)
            {
                powerEffectInstance = powerEffectPrefab != null
                    ? Instantiate(powerEffectPrefab, transform)
                    : CreateDefaultPowerEffect();
                powerEffectInstance.transform.localPosition = Vector3.zero;
                powerEffectParticles = powerEffectInstance.GetComponentsInChildren<ParticleSystem>(true);
            }

            powerEffectInstance.SetActive(true);
            foreach (ParticleSystem particles in powerEffectParticles)
            {
                particles.Play(true);
            }
        }

        private void StopPowerEffect()
        {
            if (powerEffectInstance == null) return;

            foreach (ParticleSystem particles in powerEffectParticles ?? Array.Empty<ParticleSystem>())
            {
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            powerEffectInstance.SetActive(false);
        }

        private GameObject CreateDefaultPowerEffect()
        {
            GameObject effect = new GameObject("Power Fire Effect");
            effect.transform.SetParent(transform, false);

            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = particles.main;
            main.playOnAwake = false;
            main.loop = true;
            main.duration = 1f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.75f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.9f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.28f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.22f, 0.02f, 1f));
            main.gravityModifier = -0.15f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 40;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 24f;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.4f;
            shape.radiusThickness = 0.15f;

            ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            // Unity requires x/y/z velocity curves to use the same mode.
            velocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.y = new ParticleSystem.MinMaxCurve(0.35f, 0.8f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
            color.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.95f, 0.2f), 0f),
                    new GradientColorKey(new Color(1f, 0.1f, 0.01f), 1f),
                },
                new[]
                {
                    new GradientAlphaKey(0.95f, 0f),
                    new GradientAlphaKey(0f, 1f),
                });
            color.color = new ParticleSystem.MinMaxGradient(gradient);

            ParticleSystemRenderer renderer = effect.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortMode = ParticleSystemSortMode.Distance;
            return effect;
        }

        private void CastSkillAtTarget(Monster target, SkillConfigData skill, float powerMultiplier = 1f)
        {
            float attack = GetStat(StatId.Attack).Value * Mathf.Max(1f, powerMultiplier);
            float critChance = GetStat(StatId.CritChance).Value;
            float critDamage = GetStat(StatId.CritDamage).Value;
            float lifesteal = GetStat(StatId.Lifesteal).Value;

            SkillFactory.CastSkillAsync(skill, muzzle.position, target, (monster, damage) =>
            {
                CombatDamage.DamageResult result = CombatDamage.Calculate(
                    damage, attack, critChance, critDamage, GameRng.Value);
                monster.BeHit();
                int dealt = monster.TakeDamage(result.Amount);

                if (dealt <= 0) return;

                TotalDamageDealt += dealt;
                Heal(CombatDamage.CalculateLifeSteal(dealt, lifesteal));
            }, PlayHitFeedback).Forget();
        }

        public bool TryStartOverdrive(float duration)
        {
            if (duration <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), duration, "Overdrive duration must be positive.");
            }

            if (IsDead || IsOverdriveActive)
            {
                return false;
            }

            IsOverdriveActive = true;
            OverdriveRemaining = duration;
            PowerDuration = duration;
            OverdriveActivations++;

            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
            }

            pendingAttackTarget = null;
            hasPendingAttackSkill = false;
            ApplyPowerUpgrade();
            if (Application.isPlaying)
            {
                attackCoroutine = StartCoroutine(AutoAttackEnumerator());
            }
            StartPowerEffect();
            OnOverdriveStarted?.Invoke(this);
            return true;
        }

        public void TickOverdrive(float deltaTime)
        {
            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "Overdrive delta time cannot be negative.");
            }

            if (!IsOverdriveActive)
            {
                return;
            }

            OverdriveRemaining = Mathf.Max(0f, OverdriveRemaining - deltaTime);
            if (OverdriveRemaining > 0f)
            {
                return;
            }

            IsOverdriveActive = false;
            RemovePowerUpgrade();
            StopPowerEffect();
            OnOverdriveEnded?.Invoke(this);
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || IsDead) return;
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        }

        public override int Id()
        {
            return HeroId != 0 ? HeroId : GetHashCode();
        }
    }
}

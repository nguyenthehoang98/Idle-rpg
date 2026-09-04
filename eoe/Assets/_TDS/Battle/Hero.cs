using System;
using System.Collections;
using System.Collections.Generic;
using _GameToolkit.Skills;
using _GameToolkit.Statistics;
using _TDS.GameConfig;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _TDS.Battle
{
    public class Hero : MonoBehaviour
    {
        private Dictionary<StatId, Stat> stats;
        private readonly HashSet<Monster> monsters = new HashSet<Monster>();
        private Coroutine attackCoroutine;

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
        public bool IsOverdriveActive { get; private set; }
        public float OverdriveRemaining { get; private set; }
        public int OverdriveActivations { get; private set; }

        public int CurrentHealth { get; private set; }
        public int MaxHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0;

        public Stat GetStat(StatId id)
        {
            return stats != null && stats.TryGetValue(id, out Stat stat) ? stat : null;
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
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || IsDead) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);

            Debug.Log($"[Hero:{name}] TakeDamage({damage}) hp {CurrentHealth + damage} -> {CurrentHealth}");

            if (CurrentHealth <= 0)
            {
                aliveHeroes.Remove(this);
                Debug.Log($"[Hero:{name}] DIE");

                if (attackCoroutine != null)
                {
                    StopCoroutine(attackCoroutine);
                    attackCoroutine = null;
                }

                OnDied?.Invoke();
                OnHeroDisable?.Invoke(this);
            }
        }

        private void AddMonster(Monster monster) => monsters.Add(monster);

        private void RemoveMonster(Monster monster) => monsters.Remove(monster);

        public void Initialize(HeroConfigData heroConfigData, SkillConfigData skillConfigData)
        {
            HeroId = heroConfigData.id;
            SkillConfig = skillConfigData;
            IsOverdriveActive = false;
            OverdriveRemaining = 0f;
            OverdriveActivations = 0;

            MaxHealth = Mathf.Max(1, heroConfigData.health);
            CurrentHealth = MaxHealth;

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

            if (attackCoroutine == null)
            {
                attackCoroutine = StartCoroutine(AutoAttackEnumerator());
            }
        }

        // Loop tìm & đánh theo step, chạy bằng Coroutine thay vì Update
        protected virtual IEnumerator AutoAttackEnumerator()
        {
            Debug.Log("Start AutoAttackEnumerator");
            while (true)
            {
                // AttackSpeed = số đòn / giây -> chờ 1/value
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
            float attack = GetStat(StatId.Attack).Value;
            float critChance = GetStat(StatId.CritChance).Value;
            float critDamage = GetStat(StatId.CritDamage).Value;
            float lifesteal = GetStat(StatId.Lifesteal).Value;

            // bắn projectile từ vị trí hero tới target
            SkillFactory.CastSkillAsync(SkillConfig, transform.position, target, (monster, damage) =>
            {
                CombatDamage.DamageResult result = CombatDamage.Calculate(
                    damage, attack, critChance, critDamage, UnityEngine.Random.value);
                monster.BeHit(); // kích hoạt OnBeHit (animation/hiệu ứng trúng đòn)
                int dealt = monster.TakeDamage(result.Amount);

                if (dealt <= 0) return;

                Heal(CombatDamage.CalculateLifeSteal(dealt, lifesteal));
            }).Forget();
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
            OverdriveActivations++;
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
            OnOverdriveEnded?.Invoke(this);
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || IsDead) return;
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        }
    }
}

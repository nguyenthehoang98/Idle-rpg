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

        public Stat GetStat(StatId id)
        {
            return stats != null && stats.TryGetValue(id, out Stat stat) ? stat : null;
        }

        private void OnEnable()
        {
            Monster.OnMonsterEnable += AddMonster;
            Monster.OnMonsterDisable += RemoveMonster;
        }

        private void OnDisable()
        {
            Monster.OnMonsterEnable -= AddMonster;
            Monster.OnMonsterDisable -= RemoveMonster;

            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
        }

        private void AddMonster(Monster monster) => monsters.Add(monster);

        private void RemoveMonster(Monster monster) => monsters.Remove(monster);

        public async UniTaskVoid Initialize(HeroConfigData heroConfigData, SkillConfigData skillConfigData)
        {
            SkillConfig = skillConfigData;

            stats = new Dictionary<StatId, Stat>
            {
                { StatId.MaxHealth, new Stat(heroConfigData.health) },
                { StatId.Attack, new Stat(heroConfigData.attack) },
                { StatId.AttackRange, new Stat(heroConfigData.attackRange) },
                { StatId.AttackSpeed, new Stat(heroConfigData.attackSpeed) },
                { StatId.CritChance, new Stat(heroConfigData.critChance) },
                { StatId.CritDamage, new Stat(heroConfigData.critDamage) },
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
            float bestScore = selectionType is TargetSelectionType.Farthest or TargetSelectionType.HpHighest or TargetSelectionType.AtkHighest
                ? float.MinValue : float.MaxValue;

            foreach (Monster monster in monsters)
            {
                if (monster == null || !monster.isActiveAndEnabled) continue;

                float sqrDistance = (monster.transform.position - position).sqrMagnitude;

                if (sqrDistance > sqrRange) continue;

                float score;

                switch (selectionType)
                {
                    case TargetSelectionType.Farthest: score = sqrDistance; break;
                    case TargetSelectionType.HpLowest: score = monster.CurrentHealth; break;
                    case TargetSelectionType.HpHighest: score = -monster.CurrentHealth; break;
                    case TargetSelectionType.AtkLowest: score = -monster.Attack; break;
                    case TargetSelectionType.AtkHighest: score = monster.Attack; break;
                    default: score = sqrDistance; break; // Nearest, None
                }

                // so sánh: score lớn = tốt hơn
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
            // Hook: tạo skill theo AttackId tại đây (projectile, damage...)
            Debug.Log($"{name} attack {target.name}");
        }
    }
}

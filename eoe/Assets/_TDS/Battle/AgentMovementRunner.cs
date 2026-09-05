using System;
using System.Collections.Generic;
using _GameToolkit.Avoidance;
using _GameToolkit.Entities;
using _GameToolkit.Updater;
using _TDS.GameConfig;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _TDS.Battle
{
    [Serializable]
    public sealed class AgentMovementRunner : TickRunner
    {
        [SerializeField] private AgentSimulator simulator;
        
        private Dictionary<int, Temp> container = new Dictionary<int, Temp>();
        private List<Temp> list = new List<Temp>();
        private Queue<Temp> additional = new Queue<Temp>();
        private Queue<Temp> remove = new Queue<Temp>();
        private SkillConfig skillConfig;

        public void Initialize()
        {
            simulator.Initialize();
        }

        public void SetSkillConfig(SkillConfig config)
        {
            skillConfig = config;
        }

        /// <summary>Số quái còn sống trên sân (container = agent chưa bị remove khi chết).</summary>
        public int AliveCount => container.Count;

        private void OnEnable()
        {
            Monster.OnMonsterDisable += RemoveAgent;
        }

        private void OnDisable()
        {
            Monster.OnMonsterDisable -= RemoveAgent;
        }

        // Monster chết -> bỏ agent khỏi simulator & container (không gọi lại Death)
        private void RemoveAgent(Monster monster)
        {
            int foundAgent = -1;
            Temp found = null;

            foreach (KeyValuePair<int, Temp> pair in container)
            {
                if (pair.Value.Monster != monster) continue;
                foundAgent = pair.Key;
                found = pair.Value;
                break;
            }

            if (foundAgent != -1)
            {
                container.Remove(foundAgent);
                remove.Enqueue(found); // Tick() sẽ gọi simulator.DestroyAgent
            }
        }

        public override void Tick(float deltaTime)
        {
            // monster đi tới hero sống đầu tiên (nếu có), không phải (0,0) cố định
            Vector2 goal = Vector2.zero;
            foreach (Hero hero in Hero.AliveHeroes)
            {
                if (hero == null || hero.IsDead) continue;
                goal = hero.transform.position;
                break;
            }
            simulator.Destination = goal;

            simulator.Tick(deltaTime);

            while (additional.Count > 0) list.Add(additional.Dequeue());

            for (int i = list.Count - 1; i >= 0; i--)
            {
                Temp temp = list[i];
                
                if (temp == null) { list.RemoveAt(i); continue; }

                // agent đã bị remove (monster chết) -> dọn khỏi list
                if (!container.ContainsKey(temp.Agent))
                {
                    list.RemoveAt(i);
                    continue;
                }

                simulator.SetAgentMaxSpeed(temp.Agent,
                    temp.Monster.IsStunned ? 0f : temp.MoveSpeed * temp.Monster.MoveSpeedMultiplier);

                if (simulator.TryGetAgent(temp.Agent, out var agent))
                {
                    Vector3 position = new Vector3(agent.position.x, agent.position.y);
                    
                    temp.Monster.SetPosition(position, deltaTime);

                    // monster tới đích (isStopped) -> tấn công hero gần nhất trong tầm
                    if (agent.isStopped && !temp.Monster.IsStunned && !temp.Monster.IsSilenced)
                    {
                        MonsterTickAttack(temp.Monster, deltaTime);
                    }
                }
            }
            
            while (remove.Count > 0)
            {
                Temp temp = remove.Dequeue();
                simulator.DestroyAgent(temp.Agent);
            }
        }

        public void Dispose()
        {
            simulator.Dispose();
        }

        public static float CalculateStopDistance(
            float monsterAttackRange,
            float monsterRadius,
            float heroAttackRange)
        {
            float engagementRange = Mathf.Min(monsterAttackRange, heroAttackRange);
            return Mathf.Max(monsterRadius, engagementRange);
        }

        private static float GetMaxHeroAttackRange(float fallback)
        {
            float maxRange = 0f;

            foreach (Hero hero in Hero.AliveHeroes)
            {
                if (hero == null || hero.IsDead) continue;

                var stat = hero.GetStat(StatId.AttackRange);
                if (stat != null) maxRange = Mathf.Max(maxRange, stat.Value);
            }

            return maxRange > 0f ? maxRange : fallback;
        }

        /// <summary>Monster stopped tấn công hero gần nhất trong attackRange, theo damageCooldown.</summary>
        private void MonsterTickAttack(Monster monster, float deltaTime)
        {
            Hero target = FindNearestHero(monster.transform.position, monster.AttackRange);
            if (target == null) return;

            if (TryCastSkill(monster, target, deltaTime))
            {
                return;
            }

            monster.AttackTimer -= deltaTime;
            if (monster.AttackTimer > 0f) return;

            monster.AttackTimer = monster.DamageCooldown > 0 ? monster.DamageCooldown : 1f; // missing cooldown defaults to 1 hit/s
            target.TakeDamage(monster.Attack);
        }

        private bool TryCastSkill(Monster monster, Hero target, float deltaTime)
        {
            if (skillConfig == null || !monster.TryGetReadySkill(deltaTime, out MonsterSkillConfigData skill))
            {
                return false;
            }

            if (!skillConfig.TryGetSkill(skill.skillId, out SkillConfigData config))
            {
                Debug.LogWarning($"[AgentMovementRunner] Monster {monster.name} references missing skill {skill.skillId}");
                monster.CommitSkill(skill);
                return false;
            }

            SkillFactory.CastSkillAsync(config, monster.transform.position, target, (hero, damage) =>
            {
                if (hero == null || hero.IsDead) return;

                int amount = Mathf.Max(1, Mathf.RoundToInt(
                    damage * monster.Attack * Mathf.Max(0f, skill.damageMultiplier)));
                hero.TakeDamage(amount);
            }).Forget();

            monster.CommitSkill(skill);
            return true;
        }

        private static Hero FindNearestHero(Vector3 from, float range)
        {
            Hero best = null;
            float bestSqr = range * range;

            foreach (Hero hero in Hero.AliveHeroes)
            {
                if (hero == null || hero.IsDead) continue;

                float sqr = (hero.transform.position - from).sqrMagnitude;
                if (sqr <= bestSqr)
                {
                    bestSqr = sqr;
                    best = hero;
                }
            }

            return best;
        }

        public void Create_Agent(Monster monster, SpawnScaleDefinition scaleDefinition, MonsterConfigData monsterConfigData)
        {
            int health = Mathf.CeilToInt(monsterConfigData.health * scaleDefinition.healthMultiplier);
            int attack = Mathf.CeilToInt(monsterConfigData.attack * scaleDefinition.attackMultiplier);
            int exp = Mathf.CeilToInt(monsterConfigData.exp * scaleDefinition.expMultiplier);

            monster.SetCombatData(
                health,
                attack,
                monsterConfigData.attackRange,
                monsterConfigData.damageCooldown,
                exp,
                monsterConfigData.gold,
                monsterConfigData.rank,
                monsterConfigData.skills);

            // Dừng ở khoảng cách mà cả monster và hero đều có thể đánh nhau.
            float heroAttackRange = GetMaxHeroAttackRange(monsterConfigData.attackRange);
            float stopDist = CalculateStopDistance(
                monsterConfigData.attackRange,
                monster.Radius,
                heroAttackRange);

            int agent = simulator.CreateAgent(
                monster.transform.position, monster.Radius, monsterConfigData.moveSpeed,
                stopDist
            ).agent;
            
            monster.Initialize(scaleDefinition);
            
            Temp temp = new Temp(monster, agent, monsterConfigData.moveSpeed);
            
            additional.Enqueue(temp);
            
            container.Add(agent, temp);

            ComponentManager<HealthData>.Add(agent, new HealthData(health));
        }

        public bool TryGet_AgentPosition(int agent, out Vector3 position)
        {
            if (simulator.TryGetAgent(agent, out AgentData agentData))
            {
                position = new Vector3(agentData.position.x, agentData.position.y);
                return true;
            }
            else
            {
                position = Vector3.zero;
                return false;
            }
        }
        
        public bool TryGet_Monster(int agent, out Monster monster)
        {
            if (container.TryGetValue(agent, out Temp temp))
            {
                monster = temp.Monster;
                return true;
            }
            else
            {
                monster = null;
                return false;
            }
        }

        public void Destroy_Agent(int agent, ref Action onDestroyMonsterCommand)
        {
            if (container.Remove(agent, out Temp temp))
            {
                Monster m = temp.Monster;
                onDestroyMonsterCommand += () => { m.Death(); };
                
                remove.Enqueue(temp);
            }
        }

        public void Destroy_Agent(int agent)
        {
            if (container.Remove(agent, out Temp temp))
            {
                Monster m = temp.Monster;
                m.Death();
                remove.Enqueue(temp);
            }
        }
        
        class Temp
        {
            public Monster Monster;
            public int Agent;
            public float MoveSpeed;

            public Temp(Monster monster, int agent, float moveSpeed)
            {
                Monster = monster;
                Agent = agent;
                MoveSpeed = moveSpeed;
            }
        }
    }
}
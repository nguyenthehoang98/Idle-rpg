/*using System.Collections.Generic;
using _GameToolkit.Avoidance;
using _GameToolkit.Entities;
using _GameToolkit.ResourceManagement;
using _GameToolkit.Shared;
using _TDS.Gameplay.Model;
using UnityEngine;

namespace _TDS.Gameplay.Manager
{
    public static class SkillFactory
    {
        private static Dictionary<int, CoroutineHandle> PredictedHealthCoroutines = new Dictionary<int, CoroutineHandle>();
        private static HashSet<string> assetPaths = new HashSet<string>();

        //private static SkillTickRunner Runner;
        private static int NextActionId = 0;
        
        /*public static void Initialize(SkillTickRunner runner)
        {
            SkillFactory.Runner = runner;
            SkillFactory.NextActionId = 0;
        }#1#
        
        public static void BuildSkill(SkillContext context)
        {
            if (context.Fire.UseEquipment)
            {
                // Duration tính từ quãng đường / tốc độ (config chỉ set Speed)
                float distance = Vector3.Distance(context.Fire.EquipmentMuzzle, context.Fire.ProjectileDestination);
                context.Projectile.Duration = distance / Mathf.Max(0.01f, context.Projectile.Speed);

                BuildSkill_Private(
                    context, context.Fire.EquipmentMuzzle, context.Fire.ProjectileDestination, 1
                );
                return;
            }

            PredictedTargetDamage(context);

            bool shouldContinue = true;

            Vector3 destination = context.Fire.ProjectileDestination;
            Vector3 position = context.Fire.EquipmentMuzzle;
            Vector3 direction = (destination - position).normalized;
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0);

            if (context.SpreadProjectileCount > 0)
            {
                int count = context.SpreadProjectileCount;

                float d = Mathf.CeilToInt(context.SpreadProjectileCount / 2f) * context.ProjectileDistanceStep;

                Vector3 left = position - d * perpendicular;

                int mid = count / 2;

                Vector3 right = position + d * perpendicular;

                float scaleDamage = context.SpreadDamageScale;

                for (int i = 0; i < count; i++)
                {
                    float angle = (i - (count - 1) * 0.5f) * context.ProjectileAngleStep / Mathf.Max(1, count - 1);

                    Vector3 dir = Quaternion.Euler(0, 0, angle) * direction;

                    Vector3 final;

                    if (i >= count / 2) final = right;

                    else final = left;

                    BuildSkill_Private(context, final, final + dir * 100f, mid == i ? 1 : scaleDamage);
                }

                shouldContinue = false;
            }

            if (context.ParallelProjectileCount > 0)
            {
                int count = context.ParallelProjectileCount + 1;

                float scaleDamage = context.ParallelDamageScale;

                float space = context.ProjectileDistanceStep;

                float size = context.Projectile.SizeScale;

                for (int i = 0; i < count; i++)
                {
                    float offset = (i - (count - 1) * 0.5f) * space * size;

                    Vector3 offsetPos = position + perpendicular * offset;

                    Vector3 offsetDest = destination + perpendicular * offset;

                    BuildSkill_Private(context, offsetPos, offsetDest, scaleDamage);
                }

                shouldContinue = false;
            }

            if (!shouldContinue)
            {
                BuildSkill_Private(context, position, destination, 1);
            }
        }

        private static async void BuildSkill_Private(SkillContext context, Vector3 from,
            Vector3 to, float damageScale)
        {
            Projectile projectile;
            
            if (context.Fire.UseEquipment)
            {
                // Equipment.projectile là reference tới prefab asset -> phải instantiate, không dùng trực tiếp
                GameObject equipmentPrefab = context.Fire.Equipment.projectile.gameObject;

                Pool.RegisterPool(equipmentPrefab, true); // idempotent

                projectile = Pool.Instantiate(equipmentPrefab, from, false).GetComponent<Projectile>();
            }
            else
            {
                // GetAssetCached + RegisterPool: GetAsset thường sẽ release handle ngay sau load,
                // prefab sống trong pool nhưng asset đã bị release -> hỏng khi build.
                GameObject go = await AssetLoader.GetAssetCached<GameObject>(context.AssetName);

                if (go == null)
                {
                    Debug.LogError($"[SkillManager] Projectile prefab '{context.AssetName}' not found (check Addressables)");
                    context.Fire.Equipment.StopAttack();
                    return;
                }

                if (assetPaths.Add(context.AssetName)) Pool.RegisterPool(go, true);

                projectile = Pool.Instantiate(go, from, false).GetComponent<Projectile>();
            }
            
            projectile.Initialize(from, to, context.Projectile);
            projectile.EnsureValid(out float lifetime);
            projectile.OnPhaseChanged += context.Fire.Equipment.OnProjectilePhaseChange;

            ProjectileSkillAction skillAction = new ProjectileSkillAction(
                lifetime, projectile.CollisionDetectors(), 
                context.Damage.DamageInterval, context.Damage.HitInterval, context.Damage.HitCount
            );

            skillAction.OnDamaged += info => OnDamagedFunc(context, info, projectile, damageScale);
            skillAction.OnComplete += () =>
            {
                if (context.ResetTiming == AttackResetTiming.OnSkillFinished)
                {
                    context.Fire.Equipment.StopAttack();
                }
            };

            Runner.QueueAddSkillAction(NextActionId++, skillAction);
            
            projectile.Startup();
        }

        private static bool OnDamagedFunc(SkillContext context, HitInfo hitInfo, Projectile projectile, float scaleDamage)
        {
            bool alive = MonsterTickRunner.Instance.TryGet(context.TargetEntity, out MonsterTickRunner.Data data);

            if (!alive) return false;
            
            float explosionRadius = Mathf.Max(0, context.ExplosiveRadius);

            if (explosionRadius == 0)
            {
                Vector3 textDamagePosition = hitInfo.Unique.transform.position;
               
                projectile.EvaluateTextDamagePosition(ref textDamagePosition);

                return ValidateCalculatorDamage(
                    context, projectile, textDamagePosition, context.TargetEntity,
                    hitInfo.IsLastHit, scaleDamage
                );
            }
            else
            {
                MonsterTickRunner.Instance.TryGetAgent(data.agent, out AgentData agentData);

                Vector3 explosionPosition = new Vector3(agentData.position.x, agentData.position.y);
                
                SpawnExplosive(context.ExplosiveAssetName, explosionPosition);
                
                ContactFilter2D filter = new ContactFilter2D();
                filter.useLayerMask = false;

                Collider2D[] results = new Collider2D[20];
                
                int count = Physics2D.OverlapCircle(
                    explosionPosition, explosionRadius, filter, results
                );
                
                float explosiveDamagePercent = context.ExplosiveDamageScale;
                
                for (int i = 0; i < count; i++)
                {
                    Unique unique = results[i].GetComponent<Unique>();

                    if (unique != null)
                    {
                        float explosionDamageScale = (unique == hitInfo.Unique ? 1 : explosiveDamagePercent) * scaleDamage;

                        Vector3 textDamagePosition = unique.transform.position;

                        projectile.EvaluateTextDamagePosition(ref textDamagePosition);
                        
                        return ValidateCalculatorDamage(
                            context, projectile, textDamagePosition, unique.Id(),
                            hitInfo.IsLastHit, explosionDamageScale
                        );
                    }
                }

                return true;
            }
        }

        private static bool ValidateCalculatorDamage(SkillContext context, Projectile projectile,
            Vector3 textDamagePosition, int entity,
            bool lastCollision, float scaleDamage
        )
        {
            bool alive = MonsterTickRunner.Instance.TryGet(entity, out MonsterTickRunner.Data data);

            if (!alive) return false;

            ref HealthComponent health = ref ComponentManager<HealthComponent>.Get(entity);

            if (health.CurrentHealth <= 0) return false;

            int damage = Mathf.CeilToInt(DamageFormula.DamageOutput(context, out bool critical) * scaleDamage);

            health.CurrentHealth -= damage;

            float healthPercent = health.CurrentHealth / (float)health.MaxHealth;
            
            bool killInstant = (healthPercent) <= context.InstantKillTargetBelowHealthPercent;

            if (killInstant) damage += health.CurrentHealth;
            
            data.monster.TakeDamagePost(damage);
            
            SpawnTextDamage(damage, critical, killInstant, textDamagePosition);

            if (killInstant || healthPercent <= 0)
            {
                if (lastCollision)
                {
                    projectile.OnDestroy += () =>
                    {
                        MonsterTickRunner.Instance.Remove(data);
                    };
                }
                else
                {
                    MonsterTickRunner.Instance.Remove(data);
                }
            }

            return true;
        }
        
        private static async void SpawnExplosive(string prefabName, Vector3 position)
        {
            if (string.IsNullOrEmpty(prefabName)) return;

            GameObject go = await AssetLoader.GetAssetCached<GameObject>(prefabName);

            if (assetPaths.Add(prefabName)) Pool.RegisterPool(go, true);

            GameObject o = Pool.Instantiate(go);
            
            o.transform.position = position;
        }
        
        private static async void SpawnTextDamage(int damage, bool critical, bool killInstant, Vector3 position)
        {
            string path = Path.TEXT_DAMAGE_NORMAL;

            if (killInstant)
            {
                if (critical) path = Path.TEXT_DAMAGE_CRITICAL_INSTANT_KILL;
                else path = Path.TEXT_DAMAGE_NORMAL_INSTANT_KILL;
            }
            else
            {
                if (critical) path = Path.TEXT_DAMAGE_NORMAL;
            }

            GameObject go = await AssetLoader.GetAssetCached<GameObject>(path);

            TextDamage td = Pool.Instantiate(go).GetComponent<TextDamage>();
            
            float offsetX = RandomUtils.Range(-0.3f, 0.3f);
            
            td.transform.position = position + new Vector3(offsetX, 0, 0);
            
            td.Execute(damage);
        }

        /*
     * @Damage & Hp calculate
     * Phương thức này mục đích là gán HP entity đang bị ngắm & tính hp ngay khi bắn để tránh các vũ khí đều tranh vào 1 entity
     #1#
        private static void PredictedTargetDamage(SkillContext context)
        {
            int entity = context.TargetEntity;
            
            if (!MonsterTickRunner.Instance.IsAlive(context.TargetEntity)) return;

            context.CritRate = 0;

            int damage = Mathf.CeilToInt(DamageFormula.DamageOutput(context, out bool critical));

            ref HealthComponent health = ref ComponentManager<HealthComponent>.Get(entity);

            health.PredictedHealth -= damage;

            if (PredictedHealthCoroutines.TryGetValue(entity, out CoroutineHandle coroutine))
            {
                Timing.Instance.KillCoroutinesOnInstance(coroutine);
            }

            PredictedHealthCoroutines[entity] = Timing.Instance.CallDelayedOnInstance(1, () =>
            {
                if (!MonsterTickRunner.Instance.IsAlive(context.TargetEntity)) return;

                ref HealthComponent health = ref ComponentManager<HealthComponent>.Get(entity);

                health.PredictedHealth = health.CurrentHealth;
            });
        }

        public static void Dispose() => SkillFactory.Runner = null;
    }
}*/
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Game.Configs;
using _KITSystem.Entity;
using _KITSystem.Resource;
using _KITSystem.Schedule;
using _KITSystem.SkillSystem.Core;
using _KITSystem.SkillSystem.Imp;
using UnityEngine;

namespace _Game.GamePlay.SkillSystem
{
    [Serializable]
    public class SkillTickable : Spu, ITickable
    {
        private IQuery query = new EntityQuery();

        private HashSet<GameObject> projectilesObject = new HashSet<GameObject>();
        private HashSet<string> projectilesName = new HashSet<string>();

        private static SkillTickable instance;
        
        public Task Initialize()
        {
            instance = this;
            
            return Task.CompletedTask;
        }

        public static void CastSkill(SkillData skillData, Vector3 position, Vector3 destination)
        {
            if(instance != null)
                instance.CastSkillPrivate(skillData, position, destination);
            else
                Debug.LogError("Instance AnimationTickable is null");
        }

        public static void FindTarget(FindTargetType type, Vector2 center, float radius,
            Func<int, bool> funcFilterEntity, out QueryResult result)
        {
            instance.query.FindTarget(type, center, radius, funcFilterEntity, out result);
        }

        async void CastSkillPrivate(SkillData skillData, Vector3 position, Vector3 destination)
        {
            ColliderData colliderData = skillData.collider;
            BaseCollider collider = new CircleCollider(
                query, colliderData.relativePosition, colliderData.timerTrigger,
                colliderData.duration, colliderData.radius
            );
            TrajectoryData trajectoryData = skillData.trajectory;
            BaseTrajectory trajectory = new BulletTrajectory(trajectoryData.bulletInitSpeed,
                trajectoryData.bulletInitSpeed, position, destination
            );
            GameObject go = null;
            if (!string.IsNullOrEmpty(skillData.prefabName))
            {
                go = await AssetBundleManager.GetAssetCached<GameObject>(skillData.prefabName);
                if (projectilesName.Add(skillData.prefabName))
                {
                    Pool.RegisterPool(go, true);
                    projectilesObject.Add(go);
                }

                go = Pool.Instantiate(go);
                go.transform.position = position;
            }

            DamageTickerData damageTicket = skillData.damageTicker;
            _KITSystem.SkillSystem.Core.DamageTickerType dtt = Enum.Parse<_KITSystem.SkillSystem.Core.DamageTickerType>(
                damageTicket.type.ToString()
            );
            CastProjectileAction action = new CastProjectileAction(this, skillData.lifeTime, collider, trajectory,
                DamageEntity, go, dtt, damageTicket.ticketInterval,
                colliderData.limitNumberCollision, colliderData.resetCollisionInterval
            );

            if (go != null)
            {
                Projectile p = go.GetComponent<Projectile>();
                if (p != null)
                    p.Initialize();
                else Debug.LogError("Projectile is null");
            }
            else Debug.LogError("Projectile is null");

            action.OnComplete += () =>
            {
                if (go != null)
                {
                    Projectile p = go.GetComponent<Projectile>();
                    if (p != null) p.Destroy();
                }
            };

            RequestAddAction(1, action);
        }

        private bool DamageEntity(int entity)
        {
            bool alive = EntityManager.IsEntityAlive(entity);

            if (!alive) return false;
            
            AgentTickable.Remove(entity);
            
            return true;
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (var name in projectilesName)
            {
                AssetBundleManager.UnCache(name);
            }

            projectilesName = null;

            foreach (var go in projectilesObject)
            {
                Pool.UnRegisterPool(go);
            }

            projectilesObject = null;

            instance = null;
        }
    }
}

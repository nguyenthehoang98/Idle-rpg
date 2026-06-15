using System.Collections.Generic;
using System.Threading.Tasks;
using _KITSystem.Resource;
using _KITSystem.Schedule;
using _KITSystem.SkillSystem.Core;
using _KITSystem.SkillSystem.Imp;
using UnityEngine;

namespace _Game.GamePlay.SkillSystem
{
    [System.Serializable]
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

        async void CastSkillPrivate(SkillData skillData, Vector3 position, Vector3 destination)
        {
            ColliderData colliderData = skillData.Collider;
            BaseCollider collider = new CircleCollider(
                query, colliderData.RelativePosition, colliderData.TimerTrigger,
                colliderData.Duration, colliderData.Radius
            );
            TrajectoryData trajectoryData = skillData.Trajectory;
            BaseTrajectory trajectory = new BulletTrajectory(trajectoryData.BulletInitialSpeed,
                trajectoryData.BulletAcceleration, position, destination
            );
            GameObject go = null;
            if (!string.IsNullOrEmpty(skillData.Projectile))
            {
                go = await AssetBundleManager.GetAssetCached<GameObject>(skillData.Projectile);
                if (projectilesName.Add(skillData.Projectile))
                {
                    Pool.RegisterPool(go, true);
                    projectilesObject.Add(go);
                }

                go = Pool.Instantiate(go);
                go.transform.position = position;
            }

            DamageTickerData damageTicket = skillData.DamageTicker;
            CastProjectileAction action = new CastProjectileAction(this, skillData.LifeTime, collider, trajectory,
                DamageEntity, go, damageTicket.Type, damageTicket.DamageTickerInterval,
                colliderData.LimitNumberCollisions, colliderData.ResetCollisionInterval
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

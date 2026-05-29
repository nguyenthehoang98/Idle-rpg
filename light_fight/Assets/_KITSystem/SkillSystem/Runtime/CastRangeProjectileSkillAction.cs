using _KITSystem.Resource;
using _KITSystem.SkillSystem.Config;
using Unity.Mathematics;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal sealed class CastRangeProjectileSkillAction : CastProjectileSkillAction
    {
        private BaseTrajectoryAction trajectory;
        private Transform projectile;
        private int remainingCollisions;
        
        internal CastRangeProjectileSkillAction(RangerProjectileConfig ranger, BaseTrajectoryAction trajectory, BaseShapeAction[] shapes, 
            SPU spu, TriggerConfig triggerConfig, float lifeTime) : base(shapes, spu, triggerConfig, lifeTime)
        {
            this.trajectory = trajectory;
            this.remainingCollisions = ranger.maximumCollision;
            this.projectile = KitPool.Instantiate(ranger.prefab).transform;
        }

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            float2 position = trajectory.EvaluatePosition(deltaTime);
            
            projectile.transform.position = new Vector3(position.x, position.y);
            
            for (int i = 0; i < shapes.Length; i++)
            {
                BaseShapeAction shape = shapes[i];
                var results = shape.Hit(position);
                if (results != null)
                {
                    bool hit = results.Count > 0;
#if UNITY_EDITOR
                    shape.Gizmos(new Vector3(position.x, position.y), hit ? Color.red : Color.green, 0.1f);
#endif
                    if (hit)
                    {
                        for (int i1 = 0; i1 < results.Count; i1++)
                        {
                            int entity = results[i1];

                            Damage(entity);

                            remainingCollisions--;

                            if (remainingCollisions == 0)
                            {
                                EndLifeCycle();
                                
                                return;
                            }
                        }
                    }
                }
            }
        }

        protected override void OnStop()
        {
            base.OnStop();

            if (projectile != null)
            {
                KitPool.Destroy(projectile.gameObject);

                projectile = null;
            }
        }
    }
}
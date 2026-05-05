using _KITSystem.SkillSystem.Config;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal sealed class CastMeleeProjectileAction : CastProjectileAction
    {
        private bool[] triggered;
        private Vector3 start;
        private Vector3 goal;

        internal CastMeleeProjectileAction(Vector3 start, Vector3 goal, BaseShapeAction[] shapes, Trigger trigger, float lifeTime) 
            : base(shapes, trigger, lifeTime)
        {
            this.goal = goal;
            this.start = start;
            triggered = new bool[shapes.Length];
        }

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            for (int i = 0; i < triggered.Length; i++)
            {
                if (triggered[i]) continue;
                
                var shape = shapes[i];
                if (shape.CanTrigger)
                {
                    bool hit = shape.Hit(start, out var list);
#if UNITY_EDITOR
                    shape.Gizmos(start, goal, hit ? Color.red : Color.green, deltaTime);
#endif
                    if (hit)
                    {
                        foreach (var hitId in list) Damage(hitId);
                    }
                    triggered[i] = true;
                }
            }
        }
    }
}
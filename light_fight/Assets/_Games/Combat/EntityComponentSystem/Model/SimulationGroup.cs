using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Scripting;

namespace _Games.Combat.EntityComponentSystem.Model
{
    public partial class SimulationGroup : ComponentSystemGroup
    {
        public float Iterations = 1;
        public float TimeStep = 1 / 30f;
        private float accumulator;

        protected override void OnUpdate()
        {
            float dt = World.Time.DeltaTime;
            accumulator += dt * Iterations;
            int maxSteps = 20;
            int step = 0;
            while (accumulator >= TimeStep && step < maxSteps)
            {
                base.OnUpdate();
                accumulator -= TimeStep;
                step++;
            }
            accumulator = math.min(accumulator, TimeStep * maxSteps);
        }
    }
}
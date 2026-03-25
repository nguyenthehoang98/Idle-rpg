using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Scripting;

namespace _Games.Combat.EntityComponentSystem.Model
{
    public partial class BattleSimulationGroup : ComponentSystemGroup
    {
        private float accumulator;

        protected override void OnUpdate()
        {
            var iterations = BattleStartup.BattleScaleTime;
            var timeStep = BattleStartup.BattleTimeStep;
            float dt = World.Time.DeltaTime;
            accumulator += dt * iterations;
            int maxSteps = 20;
            int step = 0;
            while (accumulator >= timeStep && step < maxSteps)
            {
                base.OnUpdate();
                accumulator -= timeStep;
                step++;
            }
            accumulator = math.min(accumulator, timeStep * maxSteps);
        }
    }
}
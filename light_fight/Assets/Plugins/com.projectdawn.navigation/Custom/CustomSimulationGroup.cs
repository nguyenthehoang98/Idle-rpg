using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectDawn.Custom
{
    public partial class CustomSimulationGroup : SimulationSystemGroup
    {
        public float Iterations { get; set; } = 1;
        public float TimeScale { get; set; } = 1;
        public float TimeStep { get; set; } = 0.0166667f;

        private float elapsedTime;
        private float accumulator;

        protected override void OnUpdate()
        {
            float timeStep = TimeStep;

            float f1 = timeStep * TimeScale;
            float f2 = f1 * Iterations;
            accumulator += f2;

            int maxSteps = 20;
            int step = 0;
            while (accumulator >= timeStep && step < maxSteps)
            {
                base.OnUpdate();
                accumulator -= timeStep;
                elapsedTime += timeStep;
                step++;
            }
            
            TimeData worldTime = new TimeData(
                elapsedTime, f1
            );
            World.Time = worldTime;

            accumulator = math.min(accumulator, timeStep * maxSteps);
        }
    }
}
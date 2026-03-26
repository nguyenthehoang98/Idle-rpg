using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;

namespace ProjectDawn.Custom
{
    public partial class CustomSimulationGroup : SimulationSystemGroup
    {
        public float Iterations = 1;
        public float TimeScale = 1;
        public float TimeStep = 0.0166667f;
        
        private float accumulator;

        protected override void OnUpdate()
        {
            float timeScale = TimeScale;
            var timeStep = TimeStep;
            var iterations = Iterations;
            TimeData worldTime = World.Time;
            TimeData scaledTime = new TimeData(
                worldTime.ElapsedTime,
                worldTime.DeltaTime * timeScale
            );
            World.Time = scaledTime;
            
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
            
            World.Time = worldTime;
            accumulator = math.min(accumulator, timeStep * maxSteps);
        }
    }
}
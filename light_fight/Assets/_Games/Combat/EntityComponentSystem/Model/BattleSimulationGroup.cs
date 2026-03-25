using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;
namespace _Games.Combat.EntityComponentSystem.Model
{
    public partial class BattleSimulationGroup : ComponentSystemGroup
    {
        private float accumulator;

        protected override void OnUpdate()
        {
            float timeScale = BattleStartup.BattleScaleTime;
            var timeStep = BattleStartup.BattleTimeStep;
            var iterations = BattleStartup.BattleIterationsUpdate;
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
using ProjectDawn.Custom;
using ProjectDawn.Navigation;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace _Games.Combat.EntityComponentSystem.System
{
    [BurstCompile]
    [UpdateBefore(typeof(SpatialGridSystem))]
    [UpdateInGroup(typeof(CustomSimulationGroup))]
    [RequireMatchingQueriesForUpdate]
    public partial struct SyncTimeScale : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            if (!Mathf.Approximately(BattleTime.ScaleTime, BattleTime.LastScaleTime))
            {
                BattleTime.LastScaleTime = BattleTime.ScaleTime;
                
                state.Dependency = new SyncLocomotionSpeedJob
                {
                    TimeScale = BattleTime.ScaleTime,
                }.ScheduleParallel(state.Dependency);
            }
        }
    }

    [BurstCompile]
    partial struct SyncLocomotionSpeedJob : IJobEntity
    {
        public float TimeScale;
        
        void Execute(ref AgentLocomotion locomotion)
        {
            locomotion.Speed = locomotion.Acceleration = TimeScale * locomotion.BaseSpeed;
        }
    }
}
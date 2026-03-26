using _Games.Combat.Data;
using ProjectDawn.Custom;
using Unity.Entities;

namespace _Games.Combat.System
{
    [UpdateInGroup(typeof(CustomSimulationGroup))]
    public partial struct GameTimeSystem : ISystem
    {
        double elapsedTime;
        Entity entity;

        public void OnCreate(ref SystemState state)
        {
            entity = state.EntityManager.CreateEntity(typeof(GameTimeData));
        }

        public void OnUpdate(ref SystemState state)
        {
            elapsedTime += state.World.Unmanaged.Time.DeltaTime;
            state.EntityManager.SetComponentData(entity, new GameTimeData { ElapsedTime = elapsedTime });
        }
    }
}
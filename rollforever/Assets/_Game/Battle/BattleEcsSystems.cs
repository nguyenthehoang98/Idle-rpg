using _KIT.Schedule;
using Leopotam.EcsLite;

namespace _Game.Battle
{
    public class BattleEcsSystems : EcsSystems, ITick
    {
        public BattleEcsSystems(EcsWorld defaultWorld, object shared = null) : base(defaultWorld, shared)
        {
        }

        public void Dispose()
        {
        }

        public void OnUpdate(float deltaTime)
        {
            Run();
        }

        public int Order => 0;
    }
}
using Unity.Mathematics;

namespace _Game.AbilitySystem
{
    public interface IAbilityLogic
    {
        void Initialize(int entity, AbilityData abilityData);

        void Startup(float2 startPos, int target);
        
        void Update(float delta);

        void Shutdown();
        
        bool IsCompleted { get; }
    }
}


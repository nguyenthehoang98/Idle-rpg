using Unity.Mathematics;
using UnityEngine;

namespace _Game.AbilitySystem
{
    public sealed class AbilityLogic
    {
        private AbilityData Data { get; }

        private float elapsed;
        private float lifeTime;

        public AbilityLogic(AbilityData data)
        {
            Data = data;
            lifeTime = data.arg.lifeTime;
        }

        public void Startup(float2 startPos, int target)
        {
            Debug.Log($"startup [{GetHashCode()}]: {Time.time}");
        }

        public void Update(float deltaTime)
        {
            elapsed += deltaTime;
        }

        public void Shutdown()
        {
            Debug.Log($"shutdown [{GetHashCode()}]: {Time.time}");
        }

        public bool IsCompleted => elapsed >= lifeTime;

        public AbilityLogic CreateInstance()
        {
            return new AbilityLogic(Data);
        }
    }
}
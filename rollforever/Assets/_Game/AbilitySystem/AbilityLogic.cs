using System;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.AbilitySystem
{
    public sealed class AbilityLogic : IDisposable
    {
        private readonly AbilityData data;
        private readonly int unitId;
        private readonly ShapeLogic shapeLogic;
        
        private float elapsed;
        private float lifeTime;

        public AbilityLogic(AbilityData data, int unitId)
        {
            this.unitId = unitId;
            this.data = data;
            shapeLogic = new ShapeLogic(data.shape);
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

        public void Dispose()
        {
            shapeLogic.Dispose();
        }

        public bool IsCompleted => elapsed >= lifeTime;

        public AbilityLogic CreateInstance(int sourceId)
        {
            return new AbilityLogic(data, sourceId);
        }
    }
}
#if UNITY_EDITOR
using _Games.Combat.EntityComponentSystem.Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace _Games.Combat.EntityComponentSystem.System
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    public partial struct GizmosSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (transform, buffers, entity) in
                     SystemAPI.Query<RefRO<LocalTransform>, DynamicBuffer<CircleBuffer>>()
                         .WithAll<MonsterTag>()
                         .WithEntityAccess())
            {
                Vector3 position = transform.ValueRO.Position;
                foreach (var buffer in buffers)
                {
                    float radius = buffer.Radius;
                    Vector3 finalPosition = position + (Vector3)buffer.Offset;
                    DrawCircleDebug(finalPosition, radius, Color.green, 12);
                }
            }
            foreach (var (transform, buffers, entity) in
                     SystemAPI.Query<RefRO<LocalTransform>, DynamicBuffer<CircleBuffer>>()
                         .WithAll<ProjectileTag>()
                         .WithEntityAccess())
            {
                Vector3 position = transform.ValueRO.Position;
                foreach (var buffer in buffers)
                {
                    float radius = buffer.Radius;
                    Vector3 finalPosition = position + (Vector3)buffer.Offset;
                    DrawCircleDebug(finalPosition, radius, Color.yellow, 12);
                }
            }
        }

        void DrawCircleDebug(Vector3 center, float radius, Color color, int segments)
        {
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * 360f / segments;
                float rad = angle * Mathf.Deg2Rad;
                Vector3 newPoint = center + new Vector3(
                    Mathf.Cos(rad) * radius,
                    Mathf.Sin(rad) * radius, 0);
                Debug.DrawLine(prevPoint, newPoint, color);
                prevPoint = newPoint;
            }
        }
    }
}
#endif
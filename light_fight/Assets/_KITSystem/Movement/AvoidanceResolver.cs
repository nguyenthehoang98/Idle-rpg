using System.Collections.Generic;
using UnityEngine;

namespace _KITSystem.Movement
{
    [System.Serializable]
    internal class AvoidanceResolver : IResolver
    {
        private List<Vector3> finalVelocities = new List<Vector3>();
        private NeighborQuery neighborQuery = new NeighborQuery();
        private float avoidRadius = 1f;
        private float avoidStrength = 1.5f;
        private float maxAvoidForce = 3.0f;

        public void Initialize()
        {
        }

        public List<Vector3> Resolve(List<Vector3> positions, List<bool> alives, List<Vector3> desiredVelocities)
        {
            neighborQuery.BuildGrid(positions);

            List<int>[] neighbors = neighborQuery.QueryNeighbors(
                positions,
                avoidRadius
            );
            
            if (finalVelocities.Count < desiredVelocities.Count)
            {
                finalVelocities.Capacity = desiredVelocities.Count;
                int delta = desiredVelocities.Count - finalVelocities.Count;
                for (int i = 0; i < delta; i++)
                {
                    finalVelocities.Add(Vector3.zero);
                }
            }
            
            int count = positions.Count;

            float avoidRadiusSq = avoidRadius * avoidRadius;

            for (int i = 0; i < count; i++)
            {
                Vector3 pos = positions[i];
                Vector3 desiredVel = desiredVelocities[i];

                Vector3 forward = desiredVel.sqrMagnitude > 0.0001f
                    ? desiredVel.normalized
                    : Vector3.zero;

                Vector3 avoidance = Vector3.zero;
                int neighborCount = 0;

                var list = neighbors[i];
                if (list == null)
                {
                    finalVelocities[i] = desiredVel;
                    continue;
                }

                for (int n = 0; n < list.Count; n++)
                {
                    int j = list[n];
                    if (j == i) continue;

                    Vector3 diff = pos - positions[j];
                    float distSq = diff.sqrMagnitude;

                    if (distSq > avoidRadiusSq || distSq < 0.00001f)
                        continue;

                    float dist = Mathf.Sqrt(distSq);
                    Vector3 dir = diff / dist;

                    // -------------------------
                    // 1. Directional weight
                    // -------------------------
                    float dirDot = Vector3.Dot(forward, -dir);
                    if (dirDot <= 0f) continue; // bỏ phía sau

                    float directionalWeight = dirDot * dirDot; // smooth hơn

                    // -------------------------
                    // 2. Relative velocity check
                    // -------------------------
                    Vector3 relativeVel = desiredVel - desiredVelocities[j];
                    float approaching = Vector3.Dot(relativeVel, diff);

                    if (approaching >= 0f) continue; // không tiến lại gần

                    // -------------------------
                    // 3. Distance weight
                    // -------------------------
                    float distWeight = 1f - (dist / avoidRadius);

                    float weight = directionalWeight * distWeight;

                    avoidance += dir * weight;
                    neighborCount++;
                }

                // -------------------------
                // Normalize force
                // -------------------------
                if (neighborCount > 0)
                {
                    avoidance /= neighborCount;
                }

                // -------------------------
                // Limit force (tránh giật)
                // -------------------------
                if (avoidance.sqrMagnitude > maxAvoidForce * maxAvoidForce)
                {
                    avoidance = avoidance.normalized * maxAvoidForce;
                }

                // -------------------------
                // Blend với velocity gốc
                // -------------------------
                Vector3 targetVelocity = desiredVel + avoidance * avoidStrength;

                // Smooth (rất quan trọng)
                finalVelocities[i] = Vector3.Lerp(
                    desiredVel,
                    targetVelocity,
                    0.5f // tweak cái này để chỉnh độ "mượt"
                );
            }

            return finalVelocities;
        }
    }
}
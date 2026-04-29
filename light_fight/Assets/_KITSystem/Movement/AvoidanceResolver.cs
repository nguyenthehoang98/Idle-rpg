using System.Collections.Generic;
using UnityEngine;

namespace _KITSystem.Movement
{
    [System.Serializable]
    internal class AvoidanceResolver : IResolver
    {
        private List<Vector3> finalVelocities = new List<Vector3>();
        private NeighborQuery neighborQuery = new NeighborQuery();
        [Range(0.1f, 0.9f)] public float stopDecelerationNormalize = 0.9f;
        public float avoidRadius = 1f;
        public float avoidStrength = 1.5f;
        public float maxAvoidForce = 3.0f;

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

            bool[] isStopped = new bool[positions.Count];
            ComputeStopState(positions, Vector3.zero, neighbors, isStopped, 2, 0.9f);

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
                    finalVelocities[i] = Vector3.MoveTowards(
                        desiredVelocities[i],
                        Vector3.zero,
                        stopDecelerationNormalize
                    );
                    continue;
                }

                if (isStopped[i])
                {
                    finalVelocities[i] = Vector3.zero;
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
        
        public static void ComputeStopState(
            List<Vector3> positions,
            Vector3 target,
            List<int>[] neighbors,
            bool[] isStopped,
            float stopDistance,
            float unitSpacing)
        {
            int count = positions.Count;

            Queue<int> queue = new Queue<int>();

            // -------------------------
            // 1. Seed (gần target)
            // -------------------------
            for (int i = 0; i < count; i++)
            {
                float dist = (target - positions[i]).sqrMagnitude;

                if (dist < stopDistance * stopDistance)
                {
                    isStopped[i] = true;
                    queue.Enqueue(i);
                }
                else
                {
                    isStopped[i] = false;
                }
            }

            // -------------------------
            // 2. BFS propagate
            // -------------------------
            while (queue.Count > 0)
            {
                int j = queue.Dequeue();
                Vector3 posJ = positions[j];

                var list = neighbors[j];
                if (list == null) continue;

                for (int n = 0; n < list.Count; n++)
                {
                    int i = list[n];
                    if (isStopped[i]) continue;

                    Vector3 toJ = posJ - positions[i];
                    float dist = toJ.magnitude;

                    if (dist < 0.0001f) continue;

                    Vector3 forward = (target - positions[i]).normalized;
                    Vector3 dir = toJ / dist;

                    // chỉ propagate về phía sau
                    float dot = Vector3.Dot(forward, dir);
                    if (dot < 0.6f) continue;

                    if (dist < unitSpacing)
                    {
                        isStopped[i] = true;
                        queue.Enqueue(i);
                    }
                }
            }
        }
    }
}
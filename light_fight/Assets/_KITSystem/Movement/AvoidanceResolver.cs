using System.Collections.Generic;
using _KITSystem.Utils;
using UnityEngine;

namespace _KITSystem.Movement
{
    [System.Serializable]
    internal class AvoidanceResolver : IResolver
    {
        public float cellSize = 1f;
        public float avoidRadius = 1.5f;
        public float avoidStrength = 2f;
        public float maxAvoidForce = 3.0f;
        [Range(0.1f, 0.9f)] public float stopDecelerationNormalize = 0.9f;
        
        private List<Vector3> finalVelocities = new List<Vector3>();
        private NeighborQuery neighborQuery;
        private bool[] isStoppedCache = new bool[0];
        private Queue<int> queueCache = new Queue<int>(256);
        private float maxAvoidForceSqr;

        public void Initialize()
        {
            neighborQuery = new NeighborQuery(cellSize);
            maxAvoidForceSqr = maxAvoidForce * maxAvoidForce;
        }

        public List<Vector3> Resolve(List<Vector3> positions, List<Vector3> destinations, List<Vector3> desiredVelocities)
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
         
            // resize nếu cần
            if (isStoppedCache.Length != count) 
                isStoppedCache = new bool[count];
            bool[] isStopped = isStoppedCache;
            ComputeStopState(positions, destinations, neighbors, isStopped, 2, 1.3f);

            for (int i = 0; i < count; i++)
            {
                Vector3 pos = positions[i];
                Vector3 desiredVel = desiredVelocities[i];

                float velSq = desiredVel.sqrMagnitude;
                Vector3 forward = velSq > 0.0001f ? desiredVel / Mathf.Sqrt(velSq) : Vector3.zero;

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
                if (avoidance.sqrMagnitude > maxAvoidForceSqr)
                {
                    avoidance = MathUtils.NormalizeSafeVec3(avoidance) * maxAvoidForce;
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
        
        private void ComputeStopState(
            List<Vector3> positions,
            List<Vector3> targets,
            List<int>[] neighbors,
            bool[] isStopped,
            float stopDistance,
            float unitSpacing)
        {
            int count = positions.Count;

            queueCache.Clear();
            var queue = queueCache;

            // -------------------------
            // 1. Seed (gần target)
            // -------------------------
            for (int i = 0; i < count; i++)
            {
                float dist = (targets[i] - positions[i]).sqrMagnitude;

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
                    float distSq = toJ.sqrMagnitude;
                    if (distSq < 0.0001f) continue;

                    float dist = Mathf.Sqrt(distSq);
                    Vector3 toTarget = targets[i] - positions[i];
                    float invLen = 1.0f / Mathf.Sqrt(toTarget.sqrMagnitude + 1e-6f);
                    Vector3 forward = toTarget * invLen;
                    Vector3 dir = toJ / dist;

                    // chỉ propagate về phía sau
                    float dot = Vector3.Dot(forward, dir);
                    if (dot < 0.7f) continue;

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
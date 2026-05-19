using System.Collections.Generic;
using _KITSystem.Utils;
using Unity.Collections;
using Unity.Mathematics;
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

        private NeighborQuery neighborQuery;

        private bool[] isStoppedCache = new bool[64];
        private int[] queueBuffer = new int[512];

        private float maxAvoidForceSqr;
        private float avoidRadiusSq;

        public void Initialize()
        {
            neighborQuery = new NeighborQuery(cellSize);
            maxAvoidForceSqr = maxAvoidForce * maxAvoidForce;
            avoidRadiusSq = avoidRadius * avoidRadius;
        }

        public void Resolve(NativeArray<float2> positions, NativeArray<float2> destinations, NativeArray<float2> desiredVelocities, NativeList<float2> outFinalVelocities)
        {
            int count = positions.Length;

            EnsureCapacityBool(ref isStoppedCache, count);
            EnsureCapacityInt(ref queueBuffer, math.max(count * 2, 512));

            neighborQuery.BuildGrid(positions);

            var neighbors = neighborQuery.QueryNeighbors(positions, avoidRadius);

            ComputeStopState(count, positions, destinations, 2f, 1.3f);

            for (int i = 0; i < count; i++)
            {
                float2 pos = positions[i];
                float2 desiredVel = desiredVelocities[i];

                float velSq = math.lengthsq(desiredVel);
                float2 forward = velSq > 0.0001f ? MathUtils.NormalizeSafe(desiredVel) : float2.zero;

                var list = neighbors[i];
                if (list == null || list.Count == 0)
                {
                    outFinalVelocities[i] = desiredVel;
                    continue;
                }

                if (isStoppedCache[i])
                {
                    outFinalVelocities[i] = float2.zero;
                    continue;
                }

                float2 avoidance = float2.zero;
                int neighborCount = 0;

                for (int n = 0; n < list.Count; n++)
                {
                    int j = list[n];
                    if (j == i) continue;

                    float2 diff = pos - positions[j];
                    float distSq = math.lengthsq(diff);

                    if (distSq > avoidRadiusSq || distSq < 0.00001f)
                        continue;

                    float2 dir = diff * math.rsqrt(distSq);

                    float dirDot = math.dot(forward, -dir);
                    if (dirDot <= 0f) continue;

                    float directionalWeight = dirDot * dirDot;

                    float2 relativeVel = desiredVel - desiredVelocities[j];
                    float approaching = math.dot(relativeVel, diff);
                    if (approaching >= 0f) continue;

                    float dist = math.sqrt(distSq);
                    float distWeight = 1f - (dist * math.rsqrt(avoidRadiusSq));

                    avoidance += dir * (directionalWeight * distWeight);
                    neighborCount++;
                }

                if (neighborCount > 0)
                {
                    avoidance *= 1f / neighborCount;
                }

                float avoidSq = math.lengthsq(avoidance);
                if (avoidSq > maxAvoidForceSqr)
                {
                    avoidance = MathUtils.NormalizeSafe(avoidance) * maxAvoidForce;
                }

                float2 targetVelocity = desiredVel + avoidance * avoidStrength;
                outFinalVelocities[i] = math.lerp(desiredVel, targetVelocity, 0.5f);
            }
        }

        private void ComputeStopState(int count, NativeArray<float2> positions, NativeArray<float2> destinations, float stopDistance, float unitSpacing)
        {
            float stopDistSq = stopDistance * stopDistance;
            float unitSpacingSq = unitSpacing * unitSpacing;

            int queueHead = 0;
            int queueTail = 0;

            for (int i = 0; i < count; i++)
            {
                float distSq = math.lengthsq(destinations[i] - positions[i]);

                if (distSq < stopDistSq)
                {
                    isStoppedCache[i] = true;
                    queueBuffer[queueTail++] = i;
                }
                else
                {
                    isStoppedCache[i] = false;
                }
            }

            while (queueHead < queueTail)
            {
                int j = queueBuffer[queueHead++];
                float2 posJ = positions[j];

                var neighbors = neighborQuery.GetCachedNeighbors(j);
                if (neighbors == null) continue;

                for (int n = 0; n < neighbors.Count; n++)
                {
                    int i = neighbors[n];
                    if (isStoppedCache[i]) continue;

                    float2 toJ = posJ - positions[i];
                    float distSq = math.lengthsq(toJ);
                    if (distSq < 0.0001f) continue;

                    float2 toTarget = destinations[i] - positions[i];
                    float invLen = math.rsqrt(math.lengthsq(toTarget) + 1e-6f);
                    float2 forward = toTarget * invLen;
                    float2 dir = toJ * math.rsqrt(distSq);

                    float dot = math.dot(forward, dir);
                    if (dot < 0.7f) continue;

                    if (distSq < unitSpacingSq)
                    {
                        isStoppedCache[i] = true;
                        if (queueTail < queueBuffer.Length)
                        {
                            queueBuffer[queueTail++] = i;
                        }
                        else
                        {
                            int newSize = math.max(queueBuffer.Length * 2, queueBuffer.Length + 256);
                            var newQueue = new int[newSize];
                            System.Array.Copy(queueBuffer, newQueue, queueBuffer.Length);
                            queueBuffer = newQueue;
                            queueBuffer[queueTail++] = i;
                        }
                    }
                }
            }
        }

        private static void EnsureCapacityBool(ref bool[] array, int required)
        {
            if (array.Length < required)
            {
                int newSize = math.max(required, array.Length * 2);
                var newArr = new bool[newSize];
                System.Array.Copy(array, newArr, array.Length);
                array = newArr;
            }
        }

        private static void EnsureCapacityInt(ref int[] array, int required)
        {
            if (array.Length < required)
            {
                int newSize = math.max(required, array.Length * 2);
                var newArr = new int[newSize];
                System.Array.Copy(array, newArr, array.Length);
                array = newArr;
            }
        }
    }
}

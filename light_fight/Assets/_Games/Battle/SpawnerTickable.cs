using System;
using System.Collections.Generic;
using System.Linq;
using _KITSystem.Grid;
using _KITSystem.Schedule;
using _KITSystem.Utils;
using RVO;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
internal class SpawnerTickable : ITickable
{
    [TitleGroup("Agent default settings")] 
    [SerializeField] private float stopDistance = 2;
    [SerializeField] private float agentRadius = 0.5f;
    [SerializeField, Range(0.1f, 0.9f)]
    private float multiplierIgnoreCheckDistance = 0.2f;
    [SerializeField, Range(0.1f, 1.0f)]
    private float deltaDistanceStuck = 0.2f;
    [TitleGroup("Debug")] 
    [SerializeField] private int total;
    
    private List<float2> positions = new List<float2>();
    private List<bool> stopped = new List<bool>();
    private List<int> stuckFrames = new List<int>();

    private bool isInitialized;
    private Simulator simulator;
    private IGridManager gridManager;
    private float stopDistanceSq;
    private float ignoreCheckNeighborDistanceSq;
    private float deltaDistanceStuckSq;
    private float elapsedTime;

    public void Tick(float deltaTime)
    {
        Initialize();
        simulator.SetTimeStep(deltaTime);
        simulator.EnsureCompleted();
        CheckSpawn(deltaTime);
        SetPreferredVelocities();
        ReachedGoal();
        simulator.DoStep();
        total = positions.Count;
        DrawLine(deltaTime);
    }

    private void DrawLine(float deltaTime)
    {
#if UNITY_EDITOR
        int count = positions.Count;
        for (int i = 0; i < count; i++)
        {
            float2 position = positions[i];
            Color color = stopped[i] ? Color.red : Color.green;
            DrawCircle(new Vector3(position.x, position.y), 0.5f, 6, color, deltaTime);
        }
#endif
    }

    private void DrawCircle(Vector3 center, float radius, int segments, Color color, float deltaTime)
    {
        Vector3 prev = center + Vector3.right * radius;
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            float angle = t * Mathf.PI * 2f;
            Vector3 next = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            Debug.DrawLine(prev, next, color, deltaTime);
            prev = next;
        }
    }

    private void Initialize()
    {
        if (!isInitialized)
        {
            gridManager = new FixedUniformGrid(1);
            simulator = new Simulator();
            simulator.SetTimeStep(0.25f);
            simulator.SetAgentDefaults(5f, 10, 10f, 10f, agentRadius, 1f, float2.zero);
            stopDistanceSq = stopDistance * stopDistance;
            float a = 2 * (1 + multiplierIgnoreCheckDistance) * agentRadius;
            ignoreCheckNeighborDistanceSq = a * a;
            deltaDistanceStuckSq = deltaDistanceStuck * deltaDistanceStuck;
            isInitialized = true;
        }
    }

    private void StopAgent(int agentId)
    {
        simulator.SetAgentMaxSpeed(agentId, 0);
        simulator.SetAgentPrefVelocity(agentId, float2.zero);
    }
    
    private void ReachedGoal()
    {
        int count = positions.Count;
        for (int i = 0; i < count; i++)
        {
            if (stopped[i]) continue;

            int agentId = i + 1;

            float2 previous = positions[i];
            float2 position = simulator.GetAgentPosition(agentId);
            positions[i] = position;
            gridManager.Insert(agentId, new Vector3(position.x, position.y));

            if (math.lengthsq(position) < stopDistanceSq)
            {
                stopped[i] = true;
                StopAgent(agentId);
                continue;
            }

            int total = gridManager.Query(new Vector3(position.x, position.y), 3, out var results);
            int frontBlockedCount = 0;

            float2 dirToGoal = MathUtils.NormalizeSafe(-position);
            for (int i1 = 0; i1 < total; i1++)
            {
                var otherId = results[i1];
                if (otherId == agentId)
                    continue;

                // chỉ quan tâm frontier
                if (!stopped[otherId - 1])
                    continue;

                float2 otherPos = simulator.GetAgentPosition(otherId);
                float2 toOther = otherPos - position;
                float distsq = math.lengthsq(toOther);
                if (distsq > ignoreCheckNeighborDistanceSq)
                    continue;

                float dot = math.dot(dirToGoal, MathUtils.NormalizeSafe(toOther));
                if (dot < 0.5f)
                    continue;

                frontBlockedCount++;
            }

            float movedDistanceSq = math.distancesq(position, previous);
            bool stuck = movedDistanceSq < deltaDistanceStuckSq;
            bool crowdedFront = frontBlockedCount >= 2;
            if (crowdedFront && stuck)
            {
                stuckFrames[i]++;
            }
            else
            {
                stuckFrames[i] = 0;
            }

            if (stuckFrames[i] >= 10)
            {
                stopped[i] = true;
                StopAgent(agentId);
            }
        }
    }

    private void SetPreferredVelocities()
    {
        int count = positions.Count;
        for (int i = 0; i < count; i++)
        {
            int agentId = i + 1;
            var position = simulator.GetAgentPosition(agentId);
            var goalVector = MathUtils.NormalizeSafe(-position);
            if (stopped[i])
            {
            }
            else
            {
                simulator.SetAgentPrefVelocity(agentId, goalVector);
            }
        }
    }

    private void CheckSpawn(float deltaTime)
    {
        elapsedTime += deltaTime;
        if (elapsedTime >= 0.1f)
        {
            elapsedTime -= 0.1f;
            Vector3 position = new Vector3(Random.value - 0.5f, Random.value - 0.5f).normalized * Random.Range(10, 14);
            Spawn(position);
        }
    }

    private void Spawn(Vector3 position)
    {
        simulator.EnsureCompleted();
        int agentId = simulator.AddAgent(new float2(position.x, position.y));
        positions.Add(new float2(position.x, position.y));
        stopped.Add(false);
        gridManager.Insert(agentId, position);
        stuckFrames.Add(0);
    }
}
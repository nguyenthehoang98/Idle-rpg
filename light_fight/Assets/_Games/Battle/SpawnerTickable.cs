using System;
using System.Collections.Generic;
using _KITSystem.Grid;
using _KITSystem.Schedule;
using _KITSystem.Utils;
using RVO;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
internal class SpawnerTickable : ITickable
{
    [SerializeField] private bool debugGizmos;
    [HideInEditorMode, DisableInPlayMode]
    [SerializeField] private bool isInitialized = false;
    [TitleGroup("Agent defaults")]
    [SerializeField] private float neighborDistance = 5;
    [SerializeField] private int maxNeighbors = 10;
    [SerializeField] private float timeHorizontal = 10; 
    [SerializeField] private float timeHorizontalDistance = 10; 
    [SerializeField] private float agentRadius = 0.5f; 
    [SerializeField] private float agentMaxSpeed = 1f;
    [TitleGroup("Grid")]
    [SerializeField] private float cellSize = 1;
    [SerializeField, Range(1.1f, 1.9f)] 
    private float ignoreNeighborsDistanceMultiplier = 1.25f;
    [SerializeField, Range(0.01f, 1f)]
    private float stuckMovementDistance = 0.15f;
    [TitleGroup("Agent debug")]
    [HideInEditorMode, DisableInPlayMode, SerializeField]
    private int agents;
    private List<float2> previousPositions = new List<float2>();
    private List<bool> stopped = new List<bool>();
    private List<int> stuckFrames = new List<int>();

    private IGridManager gridManager;
    private Simulator simulator;
    private List<int> results = new List<int>();
    private float stuckMovementDistanceSq;
    private float ignoreNeighborsDistanceSq;
    private float agentStopDistanceSq;
    private float agentRadiusSq;
    private float elapsedTime;
    
    private void Initialize()
    {
        if (!isInitialized)
        {
            gridManager = new FixedUniformGrid(cellSize);
            simulator = new Simulator();
            simulator.SetAgentDefaults(neighborDistance, maxNeighbors,
                timeHorizontal, timeHorizontalDistance,
                agentRadius, agentMaxSpeed, float2.zero
            );
            stuckMovementDistanceSq = stuckMovementDistance * stuckMovementDistance;
            agentRadiusSq = agentRadius * agentRadius;
            agentStopDistanceSq = 4 * agentRadiusSq;
            ignoreNeighborsDistanceSq = agentStopDistanceSq * ignoreNeighborsDistanceMultiplier;
            isInitialized = true;
        }
    }
    
    public void Tick(float deltaTime)
    {
        Initialize();
        simulator.SetTimeStep(deltaTime);
        CheckSpawn(deltaTime);
        DrawLine(deltaTime);
        simulator.EnsureCompleted();
        SetPreferredVelocities();
        ReachedGoal();
        simulator.DoStep();
    }

    private void DrawLine(float deltaTime)
    {
#if UNITY_EDITOR
        if (!debugGizmos) return;
        int count = previousPositions.Count;
        agents = count;
        for (int i = 0; i < count; i++)
        {
            float2 position = previousPositions[i];
            Color color = stopped[i] ? Color.red : Color.green;
            DrawCircle(new Vector3(position.x, position.y), agentRadius, 12, color, deltaTime);
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

    private void ReachedGoal()
    {
        int count = previousPositions.Count;
        for (int i = 0; i < count; i++)
        {
            int agent = ValidateAgent(i);
            
            if (stopped[i]) continue;
            
            float2 previous = previousPositions[i];
            float2 position = simulator.GetAgentPosition(agent);
            previousPositions[i] = position;
            gridManager.Insert(agent, new Vector3(position.x, position.y));

            if (math.lengthsq(position) < agentStopDistanceSq + Mathf.Epsilon)
            {
                stopped[i] = true;
                continue;
            }

            //gridManager.Query(new Vector3(position.x, position.y), cellSize * 2.5f, results);

            int frontBlockedCount = 0;
            float2 direction = MathUtils.NormalizeSafe(-position);
            foreach (var result in results)
            {
                if (result == agent) continue;
                if (!stopped[i]) continue;

                float2 otherPosition = simulator.GetAgentPosition(result);
                float2 toOther = otherPosition - position;

                //-----------------------------------
                // quá xa -> ignore
                //-----------------------------------
                if (math.lengthsq(toOther) > ignoreNeighborsDistanceSq) continue;

                //-----------------------------------
                // phải nằm phía trước
                //-----------------------------------
                float dot = math.dot(direction, MathUtils.NormalizeSafe(toOther));
                // không nằm phía trước
                if (dot < 0.5f) continue;
                
                frontBlockedCount++;
            }
            
            //-----------------------------------
            // stuck detection
            //-----------------------------------
            
            bool stuck = math.distancesq(position, previous) < stuckMovementDistanceSq + Mathf.Epsilon;
            bool crowdedFront = frontBlockedCount >= 2;
            
            //-----------------------------------
            // accumulate stuck frames
            //-----------------------------------
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
            }
        }
    }

    private void SetPreferredVelocities()
    {
        int count = previousPositions.Count;
        for (int i = 0; i < count; i++)
        {
            int agent = ValidateAgent(i);
            float2 position = simulator.GetAgentPosition(agent);
            float2 goalVector = MathUtils.NormalizeSafe(-position);
            
            if (stopped[i])
            {
                simulator.SetAgentMaxSpeed(agent, 0);
                simulator.SetAgentPrefVelocity(agent, float2.zero);
            }
            else
            {
                simulator.SetAgentPrefVelocity(agent, goalVector);
            }
        }
    }

    private void Spawn(Vector3 position)
    {
        simulator.EnsureCompleted();
        simulator.AddAgent(new float2(position.x, position.y));
        previousPositions.Add(new float2(position.x, position.y));
        stopped.Add(false);
        stuckFrames.Add(0);
    }

    private void CheckSpawn(float deltaTime)
    {
        elapsedTime += deltaTime;
        if (elapsedTime >= 0.5f)
        {
            elapsedTime -= 0.5f;
            Vector3 position = new Vector3(Random.value - 0.5f, Random.value - 0.5f).normalized * Random.Range(10, 14);
            Spawn(position);
        }
    }

    private int ValidateAgent(int i) => i + 1;
}
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
#if UNITY_EDITOR
    [SerializeField] private bool locked;
#endif
    [SerializeField] private bool shouldDestroy;
    [SerializeField] private float stopDistance = 2;
    [SerializeField] private float agentRadius = 0.5f;
    [SerializeField, Range(0.1f, 0.9f)] private float multiplierIgnoreCheckDistance = 0.2f;
    [SerializeField, Range(0.1f, 1.0f)] private float deltaDistanceStuck = 0.2f;
    [TitleGroup("Debug")] 
    [SerializeField, DisableIf("@true")] private int total;
    [SerializeField, DisableIf("@true")] private bool isInitialized;

    private Dictionary<int, AgentData> container = new Dictionary<int, AgentData>();
    private List<int> agents = new List<int>();

    private Simulator simulator;
    private IGridManager gridManager;
    private float stopDistanceSq;
    private float ignoreCheckNeighborDistanceSq;
    private float deltaDistanceStuckSq;
    private float elapsedTime;

    public int Query(float2 position, float radius, out float2[] positions)
    {
        int query = gridManager.Query(position, radius, out int[] results);
        positions = new float2[query];
        int index = 0;
        for (int i = 0; i < query; i++)
        {
            int id = results[i];
            if (container.TryGetValue(id, out AgentData data))
            {
                positions[index] = data.position;
                index++;
            }
        }

        return index;
    }

    public void Tick(float deltaTime)
    {
        Initialize();
        if (locked) return;
        simulator.SetTimeStep(deltaTime);
        simulator.EnsureCompleted();

        // todo: remove

        if (shouldDestroy) RandomRemoveAgent();
        // todo: spawn (init)
        CheckSpawn(deltaTime);
#if UNITY_EDITOR
        total = container.Count;
        DrawLine(deltaTime);
#endif
        // todo: logic update
        SetPreferredVelocities();
        ReachedGoal();
        simulator.DoStep();
    }

    private void RandomRemoveAgent()
    {
        bool should = RandomUtils.Value < 0.8f;
        if (should && agents.Count > 0)
        {
            DestroyAgent(agents[0]);
        }
    }

    public void DestroyAgent(int agent)
    {
        if (agents.Remove(agent))
        {
            simulator.EnsureCompleted();
            simulator.RemoveAgent(agent);
            gridManager.Remove(agent);
        }
    }

    private void DrawLine(float deltaTime)
    {
        foreach (int agent in agents)
        {
            AgentData value = container[agent];
            float2 position = value.position;
            Color color = value.isStopped ? Color.red : Color.green;
            DrawCircle(new Vector3(position.x, position.y), 0.5f, 6, color, deltaTime);
        }
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
        foreach (int agent in agents)
        {
            AgentData temp = container[agent];

            if (temp.isStopped) continue;

            float2 previous = temp.position;
            float2 position = simulator.GetAgentPosition(agent);
            temp.position = position;

            gridManager.Insert(agent, new float2(position.x, position.y));

            if (math.lengthsq(position) < stopDistanceSq)
            {
                temp.isStopped = true;
                container[agent] = temp;
                StopAgent(agent);
                continue;
            }

            int query = gridManager.Query(position, 3, out int[] results);
            int frontBlockedCount = 0;

            float2 dirToGoal = MathUtils.NormalizeSafe(-position);
            for (int i1 = 0; i1 < query; i1++)
            {
                int otherId = results[i1];
                if (otherId == agent)
                    continue;

                // chỉ quan tâm frontier
                if (container.TryGetValue(otherId, out AgentData other))
                {
                    if (!other.isStopped) continue;
                }

                float2 otherPos = simulator.GetAgentPosition(otherId);
                float2 toOther = otherPos - position;
                float lengthsq = math.lengthsq(toOther);
                if (lengthsq > ignoreCheckNeighborDistanceSq)
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
                temp.stuckFrames++;
            }
            else
            {
                temp.stuckFrames = 0;
            }

            if (temp.stuckFrames >= 10)
            {
                temp.isStopped = true;
                StopAgent(agent);
            }

            container[agent] = temp;
        }
    }

    private void SetPreferredVelocities()
    {
        foreach (int agent in agents)
        {
            float2 position = simulator.GetAgentPosition(agent);
            float2 goalVector = MathUtils.NormalizeSafe(-position);
            if (container[agent].isStopped)
            {
            }
            else
            {
                simulator.SetAgentPrefVelocity(agent, goalVector);
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

    private void Spawn(Vector2 position)
    {
        simulator.EnsureCompleted();
        int agent = simulator.AddAgent(position);
        agents.Add(agent);
        container.Add(agent, new AgentData
        {
            position = new float2(position.x, position.y)
        });
        gridManager.Insert(agent, position);
    }

    struct AgentData
    {
        public float2 position;
        public bool isStopped;
        public int stuckFrames;
    }
}
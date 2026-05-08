using System.Collections;
using System.Collections.Generic;
using _KITSystem.Grid;
using _KITSystem.Schedule;
using RVO;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

// chuyển thành tickable
[DefaultExecutionOrder(10)]
public class Spawner : MonoBehaviour
{
    [SerializeField] private int total;
    
    private Dictionary<int, float2> goals = new Dictionary<int, float2>();
    private Dictionary<int, float2> positions = new Dictionary<int, float2>();
    private Dictionary<int, bool> stopped = new Dictionary<int, bool>();
    private Dictionary<int, Queue<float2>> queue = new Dictionary<int, Queue<float2>>();
    private Dictionary<int, int> stuckFrames = new Dictionary<int, int>();
    private Simulator simulator;
    private IGridManager gridManager;
    private float elapsedTime;

    private void OnDrawGizmos()
    {
        if (simulator == null)
        {
            return;
        }
        
        simulator.EnsureCompleted();
        
        /*foreach (var pair in goals)
        {
            var pos = simulator.GetAgentPosition(pair.Key);
            Handles.color = stopped[pair.Key] ? Color.red : Color.green;
            Handles.DrawWireDisc(new Vector3(pos.x, pos.y), Vector3.forward, 0.5f, 0.2f);            
        }*/
    }

    private void Start()
    {
        EditorUtils.DrawGizmosSceneView(true);

        gridManager = new FixedUniformGrid(1);
        
        simulator = new Simulator();
        SetupScenario();
    }
    
    private void Update()
    {
        simulator.SetTimeStep(Time.deltaTime);
        CheckSpawn(Time.deltaTime);
        SetPreferredVelocities();
        ReachedGoal();
        simulator.DoStep();
        total = goals.Count;
    }

    private void LateUpdate()
    {
        simulator.EnsureCompleted();
    }

    private void SetupScenario()
    {
        goals = new Dictionary<int, float2>();

        // Specify the global time step of the simulation.

        // Specify the default parameters for agents that are subsequently added.
        simulator.SetAgentDefaults(5f, 10, 10f, 10f, 0.5f, 1f, new float2(0f, 0f));
    }

    private void ReachedGoal()
    {
        foreach (var pair in goals)
        {
            var agentId = pair.Key;
            var goal = pair.Value;

            if (stopped[agentId])
                continue;

            float2 previous = positions[agentId];
            float2 position = simulator.GetAgentPosition(agentId);
            positions[agentId] = position;
            gridManager.Insert(agentId, new Vector3(position.x, position.y));
            
            if (math.length(position - goal) < 2f)
            {
                stopped[agentId] = true;
                continue;
            }
            
            int total = gridManager.Query(new Vector3(position.x, position.y), 3, out var results);
            int frontBlockedCount = 0;

            float2 dirToGoal = math.normalize(-position);
            for (int i1 = 0; i1 < total; i1++)
            {
                var otherId = results[i1];
                if (otherId == agentId)
                    continue;

                // chỉ quan tâm frontier
                if (!stopped[otherId])
                    continue;

                float2 otherPos =
                    simulator.GetAgentPosition(otherId);

                float2 toOther =
                    otherPos - position;

                float dist =
                    math.length(toOther);

                //-----------------------------------
                // quá xa -> ignore
                //-----------------------------------

                if (dist > 1.25f)
                    continue;

                //-----------------------------------
                // phải nằm phía trước
                //-----------------------------------

                float dot =
                    math.dot(
                        dirToGoal,
                        math.normalize(toOther));

                // không nằm phía trước
                if (dot < 0.5f)
                    continue;

                //-----------------------------------
                // valid blocker
                //-----------------------------------

                frontBlockedCount++;
            }
            
            //-----------------------------------
            // 4. stuck detection
            //-----------------------------------

            float2 currentPosition =
                simulator.GetAgentPosition(agentId);

            float movedDistance =
                math.distance(
                    currentPosition,
                    previous);

            bool stuck =
                movedDistance < 0.15f;
            
            //-----------------------------------
            // 5. crowded front
            //-----------------------------------

            bool crowdedFront =
                frontBlockedCount >= 2;

            //-----------------------------------
            // 6. accumulate stuck frames
            //-----------------------------------

            if (crowdedFront && stuck)
            {
                if (!stuckFrames.ContainsKey(agentId))
                {
                    stuckFrames[agentId] = 0;
                }

                stuckFrames[agentId]++;
            }
            else
            {
                stuckFrames[agentId] = 0;
            }

            //-----------------------------------
            // 7. become frontier
            //-----------------------------------

            if (stuckFrames[agentId] >= 10)
            {
                stopped[agentId] = true;
            }
        }
    }

    private void SetPreferredVelocities()
    {
        // Set the preferred velocity to be a vector of unit magnitude
        // (speed) in the direction of the goal.
        foreach (var pair in goals)
        {
            var agentId = pair.Key;
            var goal = pair.Value;
            var position = simulator.GetAgentPosition(agentId);
           
            float2 goalVector = goal - position;
            if (math.lengthsq(goalVector) > 1f)
            {
                goalVector = math.normalize(goalVector);
            }

            if(stopped[agentId])
            {
                simulator.SetAgentMaxSpeed(agentId, 0);
                simulator.SetAgentPrefVelocity(agentId, float2.zero);
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
        goals.Add(agentId, float2.zero);
        positions.Add(agentId, new float2(position.x, position.y));
        stopped.Add(agentId, false);
        gridManager.Insert(agentId, position);
        queue.Add(agentId, new Queue<float2>(10));
    }
}
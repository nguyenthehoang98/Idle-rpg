using System.Collections.Generic;
using UnityEngine;

namespace _KITSystem.Movement
{
    [System.Serializable]
    public class AvoidanceResolver : IResolver
    {
        private List<Vector3> finalVelocities = new List<Vector3>();
        private Dictionary<int, List<int>> grid = new Dictionary<int, List<int>>();
        [SerializeField] private bool debugLine = false;
        [SerializeField] private float cellSize = 1.5f;
        [SerializeField] private float avoidRadius = 1.0f;
        [SerializeField] private float avoidStrength = 2.0f;
        [SerializeField] private float maxAvoidForce = 1.0f;

        public void Initialize()
        {
        }

        public List<Vector3> Resolve(List<Vector3> positions, List<bool> alives, List<Vector3> desiredVelocities)
        {
            BuildGrid(positions, alives);
            
            // xác thực lại size list
            if (finalVelocities.Count < desiredVelocities.Count)
            {
                finalVelocities.Capacity = desiredVelocities.Count;
                int delta = desiredVelocities.Count - finalVelocities.Count;
                for (int i = 0; i < delta; i++)
                {
                    finalVelocities.Add(Vector3.zero);
                }
            }

            for (int i = 0; i < positions.Count; i++)
            {
                if (!alives[i]) continue;

                Vector3 pos = positions[i];
                Vector3 avoidance = Vector3.zero;

                // 🔥 get cell
                int baseX = Mathf.FloorToInt(pos.x / cellSize);
                int baseZ = Mathf.FloorToInt(pos.z / cellSize);

                // 🔥 check neighbor cells
                foreach (var offset in NeighborOffsets)
                {
                    int nx = baseX + offset.x;
                    int nz = baseZ + offset.y;

                    int hash = nx * 73856093 ^ nz * 19349663;

                    if (!grid.TryGetValue(hash, out var list))
                        continue;

                    for (int j = 0; j < list.Count; j++)
                    {
                        int other = list[j];
                        if (other == i) continue;

                        Vector3 diff = pos - positions[other];
                        float dist = diff.magnitude;

                        if (dist < avoidRadius && dist > 0.001f)
                        {
                            float strength = (avoidRadius - dist) / avoidRadius;
                            avoidance += diff.normalized * strength;
                        }
                    }
                }

                // 🔥 clamp force
                avoidance = Vector3.ClampMagnitude(avoidance, maxAvoidForce);

                // 🔥 combine
                finalVelocities[i] = desiredVelocities[i] + avoidance * avoidStrength;

                if (debugLine)
                {
                    Debug.DrawLine(pos, pos + finalVelocities[i] * 10, Color.green);
                }
            }

            return finalVelocities;
        }

        private void BuildGrid(List<Vector3> positions, List<bool> alives)
        {
            grid.Clear();

            for (int i = 0; i < positions.Count; i++)
            {
                if (!alives[i]) continue;

                int hash = Hash(positions[i]);

                if (!grid.TryGetValue(hash, out var list))
                {
                    list = new List<int>();
                    grid[hash] = list;
                }

                list.Add(i);
            }
        }

        private int Hash(Vector3 pos)
        {
            int x = Mathf.FloorToInt(pos.x / cellSize);
            int z = Mathf.FloorToInt(pos.z / cellSize);

            return x * 73856093 ^ z * 19349663;
        }

        private static readonly Vector2Int[] NeighborOffsets =
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(1, -1),
            new Vector2Int(-1, 1), new Vector2Int(-1, -1)
        };
    }
}
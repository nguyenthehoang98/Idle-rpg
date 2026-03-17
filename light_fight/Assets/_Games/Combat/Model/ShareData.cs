using _Games.Combat.Level;
using Unity.Entities;
using UnityEngine;

namespace _Games.Combat.Model
{
    public class ShareData
    {
        public ShareData(Entity player, LevelSpawnSO levelSpawn)
        {
            Player = player;
            LevelSpawn = levelSpawn;
            var design = levelSpawn.design;
            BoxSize = CalculateBounds(design.LoopPoints(), design.CellSize);
            RadiusBonus = Mathf.Max(BoxSize.x, BoxSize.y) / 2f;
        }

        public Entity Player { get; }
        public LevelSpawnSO LevelSpawn { get; }
        public Vector2 BoxSize { get; }
        public float RadiusBonus { get; }
        public int TotalCurrentMonsterAlive { get; set; }
        
        static Vector2 CalculateBounds(Vector2[] points, Vector2 cellSize)
        {
            if (points == null || points.Length == 0)
            {
                return Vector2.zero;
            }

            Vector2 halfCell = cellSize * 0.5f;

            float minX = points[0].x;
            float maxX = points[0].x;
            float minY = points[0].y;
            float maxY = points[0].y;

            for (int i = 1; i < points.Length; i++)
            {
                Vector2 p = points[i];

                if (p.x < minX) minX = p.x;
                if (p.x > maxX) maxX = p.x;
                if (p.y < minY) minY = p.y;
                if (p.y > maxY) maxY = p.y;
            }

            // Expand theo kích thước cell
            minX -= halfCell.x;
            maxX += halfCell.x;
            minY -= halfCell.y;
            maxY += halfCell.y;

            return new Vector2(maxX - minX, maxY - minY);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace _Game.Battle.Editor
{
    public partial class ChartData
    {
        public float heightNormalize = 100;
        public Rect layoutRect;
        public Color backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        public List<Point> points = new List<Point>();
        
        public sealed class Point
        {
            public List<Vector2> points = new List<Vector2>();
            public Color color = Color.white;

            public Vector2[] NormalizePoints(float scale)
            {
                Vector2[] vector2s = new Vector2[points.Count];
                for (int i = 0; i < vector2s.Length; i++)
                {
                    vector2s[i] = points[i] * new Vector2(1f, 1f) * scale;
                }

                return vector2s;
            }
        }
    }
}
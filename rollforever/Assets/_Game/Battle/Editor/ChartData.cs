using System.Collections.Generic;
using System.Linq;
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
            public bool drawNumber;
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
        
        private Point GetPoints(List<float> list, Vector2 offset, bool drawNumber, Color color)
        {
            Point p = new Point();
            p.color = color;
            p.drawNumber = drawNumber;
            float max = list.Max();
            float total = list.Count - 1;
            for (int i = 0; i < list.Count; i++)
            {
                p.points.Add(new Vector2(i / total, list[i] / max) + offset);
            }

            return p;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Game.Battle.Checker.Editor
{
    public partial class ChartData
    {
        public float heightNormalize = 100;
        public Rect layoutRect;
        public Color backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        public List<Point> points = new List<Point>();
        
        public sealed class Point
        {
            public string floatFormat = "0";
            public bool drawNumber;
            public List<Vector2> points = new List<Vector2>();
            public List<float> values = new List<float>();
            public Color color = Color.white;

            public Vector2[] NormalizePoints(float scale)
            {
                Vector2[] normalizePoints = new Vector2[points.Count];
                for (int i = 0; i < normalizePoints.Length; i++)
                {
                    normalizePoints[i] = points[i] * new Vector2(1f, 1f) * scale;
                }

                return normalizePoints;
            }
        }
        
        private Point GetPoints(List<float> list, Vector2 offset, bool drawNumber, Color color)
        {
            Point p = new Point();
            p.color = color;
            p.values = list;
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
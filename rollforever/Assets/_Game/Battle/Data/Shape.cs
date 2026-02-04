using System.Collections.Generic;
using Unity.Mathematics;

namespace _Game.Battle.Data
{
    public class Shape
    {
        static Dictionary<int, Shape> dict = new Dictionary<int, Shape>();
        private static int globalId;

        public static Shape Insert(ShapeType shape, float radius)
        {
            globalId++;
            Shape instance = new Shape(globalId, shape, radius);
            dict[globalId] = instance;
            return instance;
        }

        public static Shape Insert(ShapeType shape, float2 size)
        {
            globalId++;
            Shape instance = new Shape(globalId, shape, size);
            dict[globalId] = instance;
            return instance;
        }

        public static void Remove(int id)
        {
            dict.Remove(id);
        }

        public static bool TryGet(int id, out Shape instance)
        {
            return dict.TryGetValue(id, out instance);
        }
        
        private Shape(int id, ShapeType type, float radius)
        {
            Id = id;
            Type = type;
            Radius = radius;
        }
        private Shape(int id, ShapeType type, float2 size)
        {
            Id = id;
            Type = type;
            HalfSize = size / 2;
        }

        public int Id { get; }
        public ShapeType Type { get; }
        public float Radius { get; }
        public float2 HalfSize { get; }
        public float2 PrevPosition { get; set; }
        public float2 CurrentPosition { get; set; }
    }

    public enum ShapeType
    {
        Circle, Box, Polygon,
    }
}
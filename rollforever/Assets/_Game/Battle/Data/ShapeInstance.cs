using System.Collections.Generic;
using Unity.Mathematics;

namespace _Game.Battle.Data
{
    public class ShapeInstance
    {
        static Dictionary<int, ShapeInstance> dict = new Dictionary<int, ShapeInstance>();
        private static int globalId;

        public static ShapeInstance Insert(ShapeType shape, float radius)
        {
            globalId++;
            ShapeInstance instance = new ShapeInstance(globalId, shape, radius);
            dict[globalId] = instance;
            return instance;
        }

        public static ShapeInstance Insert(ShapeType shape, float2 size)
        {
            globalId++;
            ShapeInstance instance = new ShapeInstance(globalId, shape, size);
            dict[globalId] = instance;
            return instance;
        }

        public static void Remove(int id)
        {
            dict.Remove(id);
        }

        public static bool TryGet(int id, out ShapeInstance instance)
        {
            return dict.TryGetValue(id, out instance);
        }
        
        private ShapeInstance(int id, ShapeType shape, float radius)
        {
            Id = id;
            Shape = shape;
            Radius = radius;
        }
        private ShapeInstance(int id, ShapeType shape, float2 size)
        {
            Id = id;
            Shape = shape;
            HalfSize = size / 2;
        }

        public int Id { get; }
        public ShapeType Shape { get; }
        public float Radius { get; }
        public float2 HalfSize { get; }
        public float2 PrevPosition { get; set; }
        public float2 CurrPosition { get; set; }
    }

    public enum ShapeType
    {
        Circle, Box, Polygon,
    }
}
using System.Collections.Generic;
using Unity.Mathematics;

namespace Geometry
{
    /// <summary>
    /// Chỉ nên lưu thông số shape, ko nên lưu runtime data -> tránh bị sửa sai chỗ
    /// </summary>
    public sealed class ShapeInstance
    {
        static Dictionary<int, ShapeInstance> dict = new Dictionary<int, ShapeInstance>();
        private static int globalId;
        
        public static ShapeInstance Empty => new ShapeInstance();

        public static ShapeInstance Create(ShapeType shape, float radius)
        {
            return new ShapeInstance(globalId, shape, radius);
        }

        public static ShapeInstance Create(ShapeType shape, float2 size)
        {
            return new ShapeInstance(globalId, shape, size);
        }

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

        public static void Remove(ShapeInstance shape)
        {
            dict.Remove(shape.Id);
        }

        public static void Remove(int id)
        {
            dict.Remove(id);
        }

        public static bool TryGet(int id, out ShapeInstance instance)
        {
            return dict.TryGetValue(id, out instance);
        }

        private ShapeInstance()
        {
        }
        
        private ShapeInstance(int id, ShapeType type, float radius)
        {
            Id = id;
            Type = type;
            Radius = radius;
        }

        private ShapeInstance(int id, ShapeType type, float2 size)
        {
            Id = id;
            Type = type;
            Size = size;
        }

        public int Id { get; }
        public ShapeType Type { get; }
        public float Radius { get; }
        public float2 Size { get; }
    }
}
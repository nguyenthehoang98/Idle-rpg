using Unity.Mathematics;

namespace Geometry
{
    public interface IShapeData
    {
        int Id { get; }
        float2 Position { get; }
        float2 HalfSizeBound { get; }
    }
    
    public struct BoxData2D : IShapeData
    {
        public BoxData2D(int id, float2 position, float2 size)
        {
            Id = id;
            Position = position;
            HalfSizeBound = size / 2;
        }

        public int Id { get; }
        public float2 Position { get; }
        public float2 HalfSizeBound { get; }
    }
}
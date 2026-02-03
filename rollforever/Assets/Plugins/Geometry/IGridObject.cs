using Unity.Mathematics;

namespace Geometry
{
    public interface IGridObject
    {
        int Id { get; }
        float2 Position { get; }
        float2 HalfSizeBound { get; }
    }
    
    public struct GridObject : IGridObject
    {
        public GridObject(int id, float2 position, float2 size)
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
using Unity.Mathematics;

namespace _Game.Battle.Data
{
    public struct GridPos
    {
        public GridPos(int id, float2 position, float radius)
        {
            Id = id;
            Position = position;
            Radius = radius;
        }

        public int Id { get; }
        public float2 Position { get; }
        public float Radius { get; }
    }
}
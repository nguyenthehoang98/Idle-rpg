using Unity.Mathematics;

namespace _Games.Battle
{
    public struct RequestCreateMonster
    {
        public readonly int MonsterID;
        public readonly float Radius;
        public readonly float2 Position;

        public RequestCreateMonster(int monsterID, float radius, float2 position)
        {
            MonsterID = monsterID;
            Radius = radius;
            Position = position;
        }
    }
}
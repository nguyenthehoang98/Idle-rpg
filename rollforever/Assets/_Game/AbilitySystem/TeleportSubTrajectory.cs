using Unity.Mathematics;

namespace _Game.AbilitySystem
{
    class TeleportSubTrajectory : ISubTrajectory
    {
        private float2 pos;
        
        public void Init(float2 startPos, float2 endPos)
        {
            this.pos = endPos;
        }

        public float2 Execute(float dt)
        {
            return pos;
        }
    }
}
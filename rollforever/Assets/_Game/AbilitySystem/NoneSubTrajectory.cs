using Unity.Mathematics;

namespace _Game.AbilitySystem
{
    class NoneSubTrajectory : ISubTrajectory
    {
        private float2 startPos;
        
        public void Init(float2 startPos, float2 endPos)
        {
            this.startPos = startPos;
        }

        public float2 Execute(float dt)
        {
            return startPos;
        }
    }
}
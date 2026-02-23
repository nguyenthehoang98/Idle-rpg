using Unity.Mathematics;

namespace _Game.Battle.AbilitySystem
{
    class VelocitySubTrajectory : ISubTrajectory
    {
        float acceleration;
        float speed;
        float2 startPos;
        float2 direction;
        
        float elapsed;
        
        public VelocitySubTrajectory(float acceleration, float speed)
        {
            this.acceleration = acceleration;
            this.speed = speed;
        }
        
        public void Init(float2 startPos, float2 endPos)
        {
            this.startPos = startPos;
            this.direction = math.normalize(endPos - startPos);
        }

        public float2 Execute(float dt)
        {
            elapsed += dt;
            float f = 0.5f * acceleration * elapsed * elapsed + speed * elapsed;
            return f * direction + startPos;
        }
    }
}
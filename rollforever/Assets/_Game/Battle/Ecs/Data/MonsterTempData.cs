using Unity.Mathematics;

namespace _Game.Battle.Ecs.Data
{
    public struct MonsterTempData
    {
        public float2 stopDistance;
#if UNITY_EDITOR
        public float2 position;
        public float2 goal;
        public float2 velocity;
        public float2 prefVelocity;
        public float2 newVelocity;
        public bool paused;
#endif
        public float threasholdVelocityElapsed;
        public bool isStopped;
        public bool canTriggerAttack;
    }
}
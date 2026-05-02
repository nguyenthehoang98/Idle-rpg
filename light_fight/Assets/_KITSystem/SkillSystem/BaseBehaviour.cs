using UnityEngine;

namespace _KITSystem.SkillSystem
{
    [System.Serializable]
    public abstract class BaseBehaviour
    {
        public abstract BehaviourType Type { get; }
    }
    
    [System.Serializable]
    public class DropStrikeBehaviour : BaseBehaviour
    {
        public int count;
        public float height;
        public float delayBetween;
        public float radius;
        public bool overrideCenter;
        public float angle;
        public override BehaviourType Type => BehaviourType.DropStrike;
    }
    
    [System.Serializable]
    public class ExplosionBehaviour : BaseBehaviour
    {
        public float radius;
        public Vector3 offset;
        public override BehaviourType Type => BehaviourType.Explosion;
    }
    
    [System.Serializable]
    public class ParallelBehaviour : BaseBehaviour
    {
        public int count;
        public Vector3 centerOffset;
        public float distanceBetween;
        public override BehaviourType Type => BehaviourType.Parallel;
    }
    
    [System.Serializable]
    public class PiercingBehaviour : BaseBehaviour
    {
        public int count;
        public override BehaviourType Type => BehaviourType.Piercing;
    }
    
    [System.Serializable]
    public class SpreadBehaviour : BaseBehaviour
    {
        public int count;
        public float angleStep;
        public float delayBetween;
        public bool randomAngle;
        public override BehaviourType Type => BehaviourType.Spread;
    }
    
    public enum BehaviourType
    {
        Spread, DropStrike, Explosion, Piercing, Parallel,
        Bounce
    }
}
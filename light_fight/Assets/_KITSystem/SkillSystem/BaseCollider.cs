using UnityEngine;

namespace _KITSystem.SkillSystem
{
    [System.Serializable]
    public abstract class BaseCollider
    {
        public abstract ColliderType Type { get; }
    }

    [System.Serializable]
    public class CircleCollider : BaseCollider
    {
        public float radius;
        public override ColliderType Type => ColliderType.Circle;
    }

    [System.Serializable]
    public class SquareCollider : BaseCollider
    {
        public Vector2 size;
        public override ColliderType Type => ColliderType.Square;
    }

    public enum ColliderType
    {
        Circle,
        Square
    }
}
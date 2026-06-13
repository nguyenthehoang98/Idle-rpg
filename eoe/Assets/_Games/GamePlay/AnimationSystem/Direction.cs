using UnityEngine;

namespace _Games.GamePlay.AnimationSystem
{
    public enum Direction
    {
        T,
        TR,
        R,
        BR,
        B,
        BL,
        L,
        TL
    }

    public static class DirectionExtensions
    {
        public static Direction GetDirection(Vector3 position, Vector3 destination)
        {
            Vector3 delta = destination - position;
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

            if (angle >= -22.5f && angle < 22.5f) return Direction.R;
            if (angle >= 22.5f && angle < 67.5f) return Direction.TR;
            if (angle >= 67.5f && angle < 112.5f) return Direction.T;
            if (angle >= 112.5f && angle < 157.5f) return Direction.TL;
            if (angle >= 157.5f || angle < -157.5f) return Direction.L;
            if (angle >= -157.5f && angle < -112.5f) return Direction.BL;
            if (angle >= -112.5f && angle < -67.5f) return Direction.B;
            return Direction.BR;
        }
    }
}
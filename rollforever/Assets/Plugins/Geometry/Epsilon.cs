using UnityEngine;

namespace Geometry
{
    public static class Epsilon
    {
        public const float Value = 1e-5f;

        public static bool Equals(float a, float b) => Mathf.Abs(a - b) < Value;
    }
}
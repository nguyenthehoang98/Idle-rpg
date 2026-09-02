using System.Runtime.CompilerServices;
using UnityEngine;

namespace _KITSystem.Utils
{
    public static class MathUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 NormalizeSafeVec3(Vector3 v)
        {
            float magSq = v.sqrMagnitude;
            if (magSq < 1e-6f) return Vector3.zero;
            return v * (1.0f / Mathf.Sqrt(magSq));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 NormalizeSafe(Vector2 v)
        {
            float magSq = v.sqrMagnitude;
            if (magSq < 1e-6f) return Vector2.zero;
            return v * (1.0f / Mathf.Sqrt(magSq));
        }
    }
}
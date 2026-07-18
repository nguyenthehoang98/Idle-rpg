using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _KITSystem.SkillSystem.Imp
{
    [Serializable]
    public struct TrajectoryData
    {
        public AnimationCurve projectileCurve;
        public AnimationCurve boomerangInitCurve;
        public AnimationCurve boomerangReturnCurve;
    }

    public enum TrajectoryType
    {
        Projectile = 1,
        Boomerang = 2,
        Stationary = 3,
    }
}
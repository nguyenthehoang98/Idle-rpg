using System;
using UnityEngine;
using UnityEngine.Splines;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _KITSystem.SkillSystem.Imp
{
    [Serializable]
    public class TrajectoryData
    {
        public AnimationCurve projectileCurve;
        public AnimationCurve boomerangInitCurve;
        public AnimationCurve boomerangReturnCurve;
        public SplineContainer spline;
    }

    public enum TrajectoryType
    {
        Projectile = 1,
        Boomerang = 2,
        Stationary = 3,
        Spline = 4,
    }
}
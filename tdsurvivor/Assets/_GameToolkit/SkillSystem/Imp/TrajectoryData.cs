using System;
using UnityEngine;

namespace _GameToolkit.SkillSystem.Imp
{
    /// <summary>
    /// Cấu hình serializable cho các loại trajectory.
    /// Lưu trong prefab/ScriptableObject, runtime dùng để build BaseTrajectory.
    /// </summary>
    [Serializable]
    public class TrajectoryData
    {
        public AnimationCurve projectileCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public AnimationCurve boomerangInitCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public AnimationCurve boomerangReturnCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public UnityEngine.Splines.SplineContainer spline;
    }

    public enum TrajectoryType
    {
        Projectile = 1,
        Boomerang = 2,
        Stationary = 3,
        Spline = 4,
    }
}

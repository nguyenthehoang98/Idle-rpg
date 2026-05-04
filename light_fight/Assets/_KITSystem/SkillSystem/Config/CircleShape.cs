using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class CircleShape : BaseShape
    {
        [TitleGroup("Circle")] public float radius = 1f;
        [TitleGroup("Circle")] public Vector3 relativePositionOfCenter;
        public override ShapeType Type => ShapeType.Circle;
    }
}
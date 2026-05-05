using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class CircleShape : BaseShape
    {
        [Indent] public float radius = 1f;
        public override ShapeType Type => ShapeType.Circle;
    }
}
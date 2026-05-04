using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class CapsuleShape : BaseShape
    {
        [TitleGroup("Capsule")] public Vector3 relativePositionOfCenter;
        [TitleGroup("Capsule")] public float radius = 1;
        [TitleGroup("Capsule")] public float height = 2;
        public override ShapeType Type => ShapeType.Capsule;
    }
}
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class CapsuleShape : BaseShape
    {
        [Indent] public float radius = 1;
        [Indent] public float height = 2;
        public override ShapeType Type => ShapeType.Capsule;
    }
}
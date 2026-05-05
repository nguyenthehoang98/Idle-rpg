using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class ConeShape : BaseShape
    {
        [Indent] public float widthTop;
        [Indent] public float widthBottom;
        [Indent] public float height;
        [Indent] public int n;
        public override ShapeType Type => ShapeType.Cone;
    }
}
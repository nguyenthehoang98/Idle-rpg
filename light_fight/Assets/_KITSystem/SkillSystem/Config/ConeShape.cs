using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class ConeShape : BaseShape
    {
        [TitleGroup("Cone")] public Vector3 relativePos;
        [TitleGroup("Cone")] public float widthTop;
        [TitleGroup("Cone")] public float widthBottom;
        [TitleGroup("Cone")] public float height;
        [TitleGroup("Cone")] public int n;
        public override ShapeType Type => ShapeType.Cone;
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace _Games.Combat.SkillSystem.Config
{
    [CreateAssetMenu(fileName = "Circle", menuName = "Game/Skill/Collider-Circle")]
    public class CircleColliderSO : BaseColliderSO
    {
        public List<Circle> list = new List<Circle>();
    
        [System.Serializable]
        public class Circle
        {
            public float radius;
            public bool adjustRadius;
            public float extraRadius;
            public AnimationCurve curve;
        }
    }
}
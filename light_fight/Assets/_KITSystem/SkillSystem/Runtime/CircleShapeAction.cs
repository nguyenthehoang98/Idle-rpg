using System.Collections.Generic;
using _KITSystem.SkillSystem.Config;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal class CircleShapeAction : BaseShapeAction
    {
        private float radius;
        private Vector2 relativePositionCenter;

        public CircleShapeAction(CircleShape hitBox) : base(hitBox)
        {
            radius = hitBox.radius;
            relativePositionCenter = hitBox.relativePositionOfCenter;
        }

        protected override bool OnHit(Vector3 position, out List<OwnGameObject> targets)
        {
            Vector2 realPosition = (Vector2)position + relativePositionCenter;
            // todo: scan objects
            targets = null;
            return false;
        }
    }
}
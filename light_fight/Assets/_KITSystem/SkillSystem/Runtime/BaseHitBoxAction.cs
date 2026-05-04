using System.Collections.Generic;
using _KITSystem.SkillSystem.Config.Model;
using UnityEngine;
using PivotType = _KITSystem.SkillSystem.Config.Action.CastProjectileAction.SquareShape.PivotType;
using BaseHitBox = _KITSystem.SkillSystem.Config.Action.CastProjectileAction.BaseHitBox;

namespace _KITSystem.SkillSystem.Runtime
{
    internal abstract partial class CastProjectileAction
    {
        internal abstract class BaseHitBoxAction
        {
            private readonly BaseHitBox hitBox;
            private float triggerTimeInSeconds;
            private float elapsed;
            
            protected BaseHitBoxAction(BaseHitBox hitBox)
            {
                this.hitBox = hitBox;
                triggerTimeInSeconds = hitBox.triggerTimeInSeconds;
            }

            public void Tick(float deltaTime)
            {
                if (CanTrigger) return;

                elapsed += deltaTime;

                if (elapsed >= triggerTimeInSeconds)
                {
                    CanTrigger = true;
                }
            }

            public bool Hit(Vector3 position, out List<OwnGameObject> targets)
            {
                if (CanTrigger)
                {
                    return OnHit(position, out targets);
                }

                targets = null;
                return false;
            }

            protected abstract bool OnHit(Vector3 position, out List<OwnGameObject> targets);

            private bool CanTrigger { get; set; }
        }
        
        internal class SquareHitBoxAction : BaseHitBoxAction
        {
            private Vector2 size;
            private Vector2 pivotRelativePosition;
            PivotType pivotType;
            
            public SquareHitBoxAction(Config.Action.CastProjectileAction.SquareShape hitBox) : base(hitBox)
            {
                size = hitBox.size;
                pivotType = hitBox.pivotType;
                pivotRelativePosition = hitBox.pivotRelativePosition;
            }

            protected override bool OnHit(Vector3 position, out List<OwnGameObject> targets)
            {
                Vector2 realPosition = GetCenterPosition(position, size, pivotType);
                // todo: scan objects
                targets = null;
                return false;
            }

            static Vector2 GetCenterPosition(Vector2 position, Vector2 size, PivotType pivot)
            {
                Vector2 half = size * 0.5f;
                switch (pivot)
                {
                    case PivotType.Center:
                        return position;
                    case PivotType.BottomLeft:
                        return position + new Vector2(half.x, half.y);
                    case PivotType.BottomRight:
                        return position + new Vector2(-half.x, half.y);
                    case PivotType.TopLeft:
                        return position + new Vector2(half.x, -half.y);
                    case PivotType.TopRight:
                        return position + new Vector2(-half.x, -half.y);
                }

                return position;
            }
        }
        
        internal class CircleHitBoxAction : BaseHitBoxAction
        {
            private float radius;
            private Vector2 relativePositionCenter;
            
            public CircleHitBoxAction(Config.Action.CastProjectileAction.CircleShape hitBox) : base(hitBox)
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
}
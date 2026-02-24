using System;
using _Game.Battle.Ecs.View;
using UnityEngine;

namespace _Game.Battle.AbilitySystem
{
    [Serializable]
    public struct CoreAbilityArg
    {
        public float lifeTime;
        public BulletView bulletPrefab;
        public Vector2 muzzleOffsetPosition;
        public AnimationName animationName;
        public FindTargetType findTarget;
        public bool isRequireTarget;
        public float maxDistanceFindTarget;
        public int maxCollision;
        public bool shouldResetCollision;
        public float resetCollisionInterval;
    }
}
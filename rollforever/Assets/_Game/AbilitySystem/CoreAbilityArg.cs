using System;
using UnityEngine;

namespace _Game.AbilitySystem
{
    [Serializable]
    public struct CoreAbilityArg
    {
        public float lifeTime;
        public GameObject bulletPrefab;
        public Vector2 muzzleOffsetPosition;
        public AnimationName animationName;
        public FindTargetType findTarget;
        public int maxCollision;
        public bool shouldResetCollision;
        public float resetCollisionInterval;
    }
}
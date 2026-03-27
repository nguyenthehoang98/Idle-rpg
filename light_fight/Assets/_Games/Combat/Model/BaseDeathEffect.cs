using System;
using UnityEngine;

namespace _Games.Combat.Model
{
    public abstract class BaseDeathEffect : MonoBehaviour
    {
        [SerializeField] protected Transform visual;
        [SerializeField] protected float earlyPlayTime;

        public float EarlyPlayTime => earlyPlayTime;

        public abstract void Play(Action onComplete);
    }
}
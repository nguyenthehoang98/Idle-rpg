using System;
using UnityEngine;

namespace _Games.Battle
{
    public interface IAttractorView
    {
        IAttractorView Instantiate(Transform parent);
        void MoveTo(Vector3 start, Vector3 target, Vector3 rot, float delay,
            float duration, float radius, float offsetY, float smooth, Action onComplete);
    }
}
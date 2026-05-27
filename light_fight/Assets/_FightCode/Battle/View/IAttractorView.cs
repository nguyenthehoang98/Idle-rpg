using System;
using UnityEngine;

namespace _FightCode.Battle.View
{
    public interface IAttractorView
    {
        IAttractorView Instantiate(Transform parent);
        void MoveTo(Vector3 start, Vector3 target, Vector3 rot, float flyToTargetDelay,
            float duration, float radius, float offsetY, float smooth, Action onComplete);
    }
}
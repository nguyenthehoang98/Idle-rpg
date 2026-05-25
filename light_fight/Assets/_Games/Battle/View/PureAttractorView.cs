using System;
using UnityEngine;

namespace _Games.Battle.View
{
    public class PureAttractorView : IAttractorView
    {
        public IAttractorView Instantiate(Transform parent)
        {
            return new PureAttractorView();
        }

        public void MoveTo(Vector3 start, Vector3 target, Vector3 rot, float flyToTargetDelay, float duration, float radius,
            float offsetY, float smooth, Action onComplete)
        {
            
        }
    }
}
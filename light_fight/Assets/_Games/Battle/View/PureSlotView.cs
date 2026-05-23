using UnityEngine;

namespace _Games.Battle.View
{
    [System.Serializable]
    public class PureSlotView : ISlotView
    {
        public ISlotView Instantiate(Transform parent, Vector3 localEulerAngles)
        {
            return new PureSlotView();
        }

        public float Initialize(float timeScale)
        {
            return 0;
        }

        public float Play(float timeScale)
        {
            return 0;
        }

        public float Activate(float timeScale)
        {
            return 0;
        }

        public float Deactivate(float timeScale)
        {
            return 0;
        }
    }
}
using UnityEngine;

namespace _FightCode.Battle.View
{
    [System.Serializable]
    public class PureSlotView : ISlotView
    {
        public ISlotView Instantiate(Transform parent, Vector3 localEulerAngles)
        {
            return new PureSlotView();
        }

        public void Initialize(float timeScale)
        {
            
        }

        public float Play(float timeScale)
        {
            return 0;
        }

        public void Activate(float delayActivate, float timeScale)
        {
            
        }

        public void Deactivate(float delayDeactivate, float timeScale)
        {
            
        }

        public void Stack(int totalStack, float lerpDuration, float timeScale)
        {
            
        }

        public Transform WeaponRoot => null;

        public Vector3 WorldPosition(int stack)
        {
            return Vector3.zero;
        }

        public Vector3 WorldEulerAngles(int stack)
        {
            return Vector3.zero;
        }
    }
}
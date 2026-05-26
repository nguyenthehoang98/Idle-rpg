using UnityEngine;

namespace _Games.Battle.View
{
    public class PureWeaponView : IWeaponView
    {
        public IWeaponView Instantiate(int xPivotAngle, int zPivotAngle, ISlotView slotView)
        {
            return new PureWeaponView();
        }

        public void Play(float timeScale)
        {
            
        }

        public float Activate(float delayActivate, float timeScale)
        {
            return 0;
        }

        public float Deactivate(float delayActivate, float timeScale)
        {
            return 0;
        }

        public void Rotate(Vector3 goal, float duration, float timeScale, bool needUpdatePosition)
        {
        }
    }
}
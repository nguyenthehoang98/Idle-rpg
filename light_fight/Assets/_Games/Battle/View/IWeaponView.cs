using UnityEngine;

namespace _Games.Battle.View
{
    public interface IWeaponView
    {
        IWeaponView Instantiate(int xPivotAngle, int zPivotAngle, ISlotView slotView);
        void Play(float timeScale);
        float Activate(float delayActivate, float timeScale);
        float Deactivate(float delayActivate, float timeScale);
        void Rotate(Vector3 goal, float duration, float timeScale, bool needUpdatePosition);
    }
}
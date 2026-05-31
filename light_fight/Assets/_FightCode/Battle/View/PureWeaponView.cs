using _FightCode.Battle.Model;
using UnityEngine;

namespace _FightCode.Battle.View
{
    public class PureWeaponView : IWeaponView
    {
        public IWeaponView Instantiate(int order, BattleSetting setting, int xPivotAngle, int zPivotAngle, ISlotView slotView)
        {
            return new PureWeaponView();
        }

        public Vector3 MuzzlePosition => Vector3.zero;

        public void Play(float timeScale)
        {
            
        }

        public void Equip(int weaponId, int weaponLevel)
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
using _FightCode.Battle.Model;
using UnityEngine;

namespace _FightCode.Battle.View
{
    public interface IWeaponView
    {
        IWeaponView Instantiate(int order, BattleSetting setting, int xPivotAngle, int zPivotAngle, ISlotView slotView);
        Vector3 MuzzlePosition { get; }
        void Play(float timeScale);
        void Equip(int weaponId, int weaponLevel);
        float Activate(float delayActivate, float timeScale);
        float Deactivate(float delayActivate, float timeScale);
        void Rotate(Vector3 goal, float duration, float timeScale, bool needUpdatePosition);
    }
}
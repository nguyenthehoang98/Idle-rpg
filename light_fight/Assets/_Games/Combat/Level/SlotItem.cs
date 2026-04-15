using System;
using _Games.Combat.Equipment;
using _Games.Config;
using MoreMountains.Feedbacks;
using PrimeTween;
using Unity.Mathematics;
using UnityEngine;

namespace _Games.Combat.Level
{
    public class SlotItem : MonoBehaviour
    {
        [SerializeField] private Transform parent;
        [Header("Feel")] 
        [SerializeField] private MMF_Player equipFeedback;
        [SerializeField] private MMF_Player triggerFeedback;
        [SerializeField] private MMF_Player untriggerFeedback;

        private Tween rotationTween;

        public WeaponData WeaponData { get; private set; }
        public int WeaponLevel {get; private set;}

        private BaseWeaponItem Item { get; set; }

        public bool IsEquipped => Item != null;

        public async void Equip(WeaponData weaponData, int weaponLevel)
        {
            WeaponData = weaponData;
            WeaponLevel = weaponLevel; 
            equipFeedback.PlayFeedbacks();
            Item = await BaseWeaponItem.Build(parent, weaponData, weaponLevel);
            Debug.Log(@"Bắn 1 cái vfx hình vuông ở item");
        }

        public void Trigger() => triggerFeedback.PlayFeedbacks();

        public void UnTrigger() => untriggerFeedback.PlayFeedbacks();

        public void Reset()
        {
            rotationTween.Stop();
            UnTrigger();

            // reset về idle
            if (Item != null)
            {
                Transform target = Item.transform;
                target.rotation = Quaternion.Euler(0, 0, 0);
                target.localScale = Vector3.one;
            }
        }

        public void Rotation(float rad, float time, Action onComplete)
        {
            rotationTween.Stop();
            Transform target = Item.transform;
            rotationTween = Tween.LocalRotation(target, quaternion.Euler(0, 0, rad), time)
                .OnUpdate(target, (trans, t) =>
                {
                    float currentAngle = trans.localEulerAngles.z;
                    if (currentAngle > 180)
                        currentAngle -= 360;
                    bool needFlip = currentAngle > 90 || currentAngle < -90;
                    target.localScale = new Vector3(1, needFlip ? -1 : 1, 1);
                })
                .OnComplete(() =>
                {
                    Item.Execute(onComplete);
                });
        }
    }
}
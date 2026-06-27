using _Game.Configs;
using _KITSystem.Resource;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.GamePlay.View
{
    public class EquipmentActivation : MonoBehaviour
    {
        [SerializeField] private EquipmentSlot[] slots;

        public void IdleAnimation(int idx, float speed)
        {
            EquipmentSlot sl = slots[idx];
            sl.Speed = speed;
            sl.PlayIdle();
        }

        public void ReleaseAnimation(int idx, float speed)
        {
            EquipmentSlot sl = slots[idx];
            sl.Speed = speed;
            sl.PlayRelease();
        }

        public async void SetWeapon(int idx, WeaponData data)
        {
            EquipmentSlot sl = slots[idx];
            sl.SetIcon(await AssetBundleManager.GetAssetCached<Sprite>(data.iconName));
        }
    }
}
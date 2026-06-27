using _Game.Configs;
using _KITSystem.Resource;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.GamePlay.View
{
    public class EquipmentActivation : MonoBehaviour
    {
        private static readonly int SlotIdle = Animator.StringToHash("slot_idle");
        private static readonly int SlotRelease = Animator.StringToHash("slot_release");

        [SerializeField] private Animator[] animators = new Animator[4];
        [SerializeField] private Image[] imgEquipments;

        public void IdleAnimation(int idx, float speed)
        {
            Animator anim = animators[idx];
            anim.Play(SlotIdle);
            anim.speed = speed;
        }

        public void ReleaseAnimation(int idx, float speed)
        {
            Animator anim = animators[idx];
            anim.Play(SlotRelease);
            anim.speed = speed;
        }

        public async void SetWeapon(int idx, WeaponData data)
        {
            Sprite icon = await AssetBundleManager.GetAssetCached<Sprite>(data.iconName);
            imgEquipments[idx].sprite = icon;
        }
    }
}
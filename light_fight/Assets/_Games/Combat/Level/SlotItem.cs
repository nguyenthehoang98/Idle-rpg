using _Games.Config;
using MoreMountains.Feedbacks;
using PrimeTween;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Games.Combat.Level
{
    public class SlotItem : MonoBehaviour
    {
        [Header("Setting")] 
        [SerializeField] private SortingGroup sortingGroup;
        [SerializeField] private Transform parent;
        [Header("Feel")] 
        [SerializeField] private MMF_Player equipFeedback;
        [Header("Sprite")]
        [SerializeField] private SpriteRenderer border;
        [SerializeField] private SpriteRenderer body;

        private Color originalBorderColor = new Color(51f / 255f, 51f / 255f, 51f / 255f, 128f / 255f);
        private Color highlightBorderColor = new Color(1f, 1f, 0f, 200f / 255f);
        private Tween tween;

        public WeaponData WeaponData { get; private set; }
        public int WeaponLevel {get; private set;}

        private WeaponItem Item { get; set; }

        public bool IsEquipped => Item != null;

        public async void Equip(WeaponData weaponData, int weaponLevel)
        {
            WeaponData = weaponData;
            WeaponLevel = weaponLevel; 
            equipFeedback.PlayFeedbacks();
            Item = await WeaponItem.Build(parent, weaponData, weaponLevel);
        }

        // Game ko cho unequip
        public void UnEquip()
        {
            body.color = Color.gray;
            if (Item != null)
            {
                Object.Destroy(Item.gameObject);
                Item = null;
            }
        }

        public void SetSortingOrder(int order) => sortingGroup.sortingOrder = order;

        public void Trigger() => border.color = highlightBorderColor;

        public void UnTrigger() => border.color = originalBorderColor;

        public void Reset()
        {
            tween.Stop();
            UnTrigger();

            // reset về idle
            if (Item != null)
            {
                Transform target = Item.transform;
                target.rotation = Quaternion.Euler(0, 0, 0);
                target.localScale = Vector3.one;
            }
        }

        public void Rotation(float rad, float time)
        {
            tween.Stop();
            Transform target = Item.transform;
            tween = Tween.LocalRotation(target, quaternion.Euler(0, 0, rad), time)
                .OnUpdate(target, (trans, t) =>
                {
                    float currentAngle = trans.localEulerAngles.z;
                    if (currentAngle > 180)
                        currentAngle -= 360;
                    bool needFlip = currentAngle > 90 || currentAngle < -90;
                    target.localScale = new Vector3(1, needFlip ? -1 : 1, 1);
                });
        }
    }
}
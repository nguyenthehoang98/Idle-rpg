using PrimeTween;
using Unity.Mathematics;
using UnityEngine;

namespace _Games.Combat.Level
{
    public class SlotView : MonoBehaviour
    {
        [Header("Setting")] 
        [SerializeField] private Canvas canvas;
        [SerializeField] private Vector2 slotSize = new Vector2(80, 80);
        [Header("Sprite")]
        [SerializeField] private SpriteRenderer border;
        [SerializeField] private SpriteRenderer body;

        private Color originalBorderColor = new Color(51f / 255f, 51f / 255f, 51f / 255f, 128f / 255f);
        private Color highlightBorderColor = new Color(1f, 1f, 0f, 200f / 255f);
        private Tween tween;

        public WeaponItemView ItemView { get; private set; }

        public bool IsEquipped => ItemView != null;

        public void Equip(WeaponItemView itemView)
        {
            body.color = Color.black;

            ItemView = itemView;
            RectTransform rect = itemView.GetComponent<RectTransform>();
            Vector2 scale = new Vector2(slotSize.x / rect.rect.width, slotSize.y / rect.rect.height);
            itemView.transform.SetParent(canvas.transform);
            itemView.transform.localPosition = Vector3.zero;
            itemView.transform.localScale = scale;
        }

        public void UnEquip()
        {
            body.color = Color.gray;
            ItemView = null;
        }

        public void SetOrderCanvas(int order)
        {
            canvas.sortingOrder = order;
        }

        public void Trigger() => border.color = highlightBorderColor;

        public void UnTrigger() => border.color = originalBorderColor;

        public void Rotation(float rad, float time)
        {
            tween.Stop();
            Transform target = ItemView.Icon;
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
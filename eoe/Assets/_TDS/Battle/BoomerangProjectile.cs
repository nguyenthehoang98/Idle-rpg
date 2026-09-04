using UnityEngine;

namespace _TDS.Battle
{
    /// <summary>
    /// Đạn boomerang: bay ra 1 đoạn (outboundTime) rồi tự đảo hướng bay ngược về
    /// điểm xuất phát — đi rồi quay về. Đạn tự quản lý, Skill không cần biết.
    /// </summary>
    public class BoomerangProjectile : Projectile
    {
        [Tooltip("Thời gian bay ra (giây) trước khi quay về. <=0: dùng nửa thời gian sống.")]
        public float outboundTime = -1f;

        private Vector3 startPosition;
        private float timeBeforeReturn;
        private bool returned;

        protected override void OnSetup(Vector3 from)
        {
            startPosition = from;
            timeBeforeReturn = outboundTime > 0 ? outboundTime : totalDuration * 0.5f;
            returned = false;
        }

        protected override void UpdateMotion(float dt)
        {
            if (elapsed < timeBeforeReturn) return; // vẫn đang bay ra xa

            if (returned) return; // đã đổi hướng về rồi, bay thẳng tiếp

            returned = true;

            Vector3 back = startPosition - (Vector3)transform.position;
            if (back.sqrMagnitude > 1e-6f)
            {
                direction = back.normalized;
                RotateToDirection();
            }
        }
    }
}

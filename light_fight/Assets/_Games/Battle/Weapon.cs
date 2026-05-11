using _KITSystem.Utils;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Games.Battle
{
    public class Weapon : MonoBehaviour
    {
        [TitleGroup("Feedback")] 
        [SerializeField] private float delayFocus = 0.1f;
        [SerializeField] private MMF_Player activeFeedback;
        [SerializeField] private MMF_Player inactiveFeedback;

        [TitleGroup("Element")]
        [SerializeField] private Transform pivot;
        [SerializeField] private new Transform renderer; // animator/animation

        private Tweener tweener;
        private Vector3 localPosition;
        private Vector3 localRotation;
        private Vector3 localScale;
        
        private void Awake()
        {
            localPosition = transform.localPosition;
            localRotation = pivot.localEulerAngles;
            localScale = pivot.localScale;
        }

        public float Active()
        {
            activeFeedback.PlayFeedbacks();
            return activeFeedback.TotalDuration;
        }
    
        public float Inactive()
        {
            inactiveFeedback.PlayFeedbacks();
            return inactiveFeedback.TotalDuration;
        }

        public void Focus(Vector3 goal)
        {
            Debug.Log($"Kiểm tra khi Abs(Z) đủ 180 thì bắt đầu flip X");
            this.WaitInvoke(delayFocus, () =>
            {
                Vector3 position = pivot.position;
                Vector3 direction = goal - position;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Vector3 eulerAngles = pivot.eulerAngles;
                float currentAngle = eulerAngles.z;
                float delta = Mathf.DeltaAngle(currentAngle, angle);
                float endAngle = currentAngle + delta;
                if (tweener != null && tweener.IsPlaying()) tweener.Kill();

                tweener = DOVirtual.Float(currentAngle, endAngle, 0.1f, value =>
                {
                    eulerAngles.z = value;
                    pivot.eulerAngles = eulerAngles;
                }).SetEase(Ease.InSine);
            });
        }

        public float StopFocus()
        {
            float angle = localRotation.z;
            float beginAngle = pivot.localEulerAngles.z;
            if (tweener != null && tweener.IsPlaying()) tweener.Kill();
            tweener = DOVirtual.Float(beginAngle, angle, 0.1f, value =>
            {
                pivot.localEulerAngles = new Vector3(0, 0, value);
            });
            return 0.1f;
        }
    }
}
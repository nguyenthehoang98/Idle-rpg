using _KITSystem.Utils;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace _Games.Battle
{
    public class Weapon : MonoBehaviour
    {
        [TitleGroup("Feedback")] [SerializeField]
        private float backPhaseDuration = 0.1f;

        [SerializeField] private float rotatePhaseDuration = 0.1f;
        [SerializeField] private float delayFocus = 0.1f;
        [SerializeField] private MMF_Player activeFeedback;
        [SerializeField] private MMF_Player inactiveFeedback;

        [TitleGroup("Element")] [SerializeField]
        private Transform pivot;

        [SerializeField] private new Transform renderer; // animator/animation

        private float feedbackScaleTime = 1;
        private Tweener tweener;
        private Vector3 localRotation;
        private Vector3 localPosition;

        private void Awake()
        {
            localRotation = pivot.localEulerAngles;
            localPosition = pivot.localPosition;
        }

        public void Initialize(float timeScale) => feedbackScaleTime = timeScale;

        public float Active()
        {
            activeFeedback.TimescaleMultiplier = feedbackScaleTime;
            activeFeedback.PlayFeedbacks();
            return activeFeedback.TotalDuration / feedbackScaleTime;
        }

        public float Inactive()
        {
            inactiveFeedback.TimescaleMultiplier = feedbackScaleTime;
            inactiveFeedback.PlayFeedbacks();
            return inactiveFeedback.TotalDuration / feedbackScaleTime;
        }

        public void Focus(Vector3 goal)
        {
            this.WaitInvoke(delayFocus / feedbackScaleTime, () =>
            {
                Vector3 position = pivot.position;
                Vector3 direction = goal - position;
                direction.z = 0;
                if (direction.sqrMagnitude < 0.0001f) return;

                float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                float currentAngle = pivot.eulerAngles.z;
                float delta = Mathf.DeltaAngle(currentAngle, targetAngle);
                float endAngle = currentAngle + delta;

                bool needOffset = Mathf.Abs(delta) > 90f;
                Vector3 offsetDir = needOffset ? -pivot.right * 0.15f : Vector3.zero;

                Vector3 originalLocalPos = pivot.localPosition;
                Vector3 targetLocalPos = originalLocalPos + offsetDir;

                if (tweener != null && tweener.IsPlaying()) tweener.Kill();
                tweener = DOVirtual.Float(0, 1, rotatePhaseDuration / feedbackScaleTime, value =>
                {
                    float eased = EaseInOutSine(value);
                    float a = math.lerp(currentAngle, endAngle, eased);
                    pivot.eulerAngles = new Vector3(0, 0, a);

                    if (needOffset)
                    {
                        pivot.localPosition = Vector3.Lerp(originalLocalPos, targetLocalPos, eased);
                    }
                }).SetEase(Ease.Linear);
            });
        }

        public float StopFocus()
        {
            Vector3 beginLocalPos = pivot.localPosition;
            bool flag = beginLocalPos != localPosition;

            float targetAngle = localRotation.z;
            float beginAngle = pivot.localEulerAngles.z;
            float delta = Mathf.DeltaAngle(beginAngle, targetAngle);
            float endAngle = beginAngle + delta;

            if (tweener != null && tweener.IsPlaying()) tweener.Kill();
            tweener = DOVirtual.Float(0, 1, backPhaseDuration / feedbackScaleTime, value =>
            {
                float eased = EaseInOutSine(value);
                float a = math.lerp(beginAngle, endAngle, eased);
                pivot.localEulerAngles = new Vector3(0, 0, a);

                if (flag)
                {
                    pivot.localPosition = Vector3.Lerp(beginLocalPos, localPosition, eased);
                }
            }).SetEase(Ease.Linear);

            return backPhaseDuration / feedbackScaleTime;
        }

        private static float EaseInOutSine(float t)
        {
            return -(math.cos(math.PI * t) - 1) * 0.5f;
        }
    }
}
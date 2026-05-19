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
        private Vector3 localPosition;
        private Vector3 localRotation;

        private void Awake()
        {
            localPosition = pivot.localPosition;
            localRotation = pivot.localEulerAngles;
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
            this.WaitInvoke(delayFocus, () =>
            {
                Vector3 position = pivot.position;
                Vector3 direction = goal - position;
                Vector3 eulerAngles = pivot.eulerAngles;

                float currentAngle = eulerAngles.z;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                float delta = Mathf.DeltaAngle(currentAngle, angle);
                float endAngle = currentAngle + delta;

                bool needOffset = Mathf.Abs(delta) > 90f;

                Vector3 originalLocalPos = pivot.localPosition;
                Vector3 targetLocalPos = needOffset ? originalLocalPos + Vector3.up * 0.1f : originalLocalPos;

                if (tweener != null && tweener.IsPlaying()) tweener.Kill();
                tweener = DOVirtual.Float(0, 1, rotatePhaseDuration, value =>
                {
                    eulerAngles.z = value;
                    pivot.eulerAngles = eulerAngles;

                    if (needOffset)
                    {
                        Vector3 v = Vector3.Lerp(originalLocalPos, targetLocalPos, value);
                        pivot.localPosition = v;
                    }
                }).SetEase(Ease.InSine);
            });
        }

        public float StopFocus()
        {
            Vector3 beginLocalPos = pivot.localPosition;
            bool flag = beginLocalPos != localPosition;
            
            float angle = localRotation.z;
            float beginAngle = pivot.localEulerAngles.z;
            
            if (tweener != null && tweener.IsPlaying()) tweener.Kill();
            tweener = DOVirtual.Float(0, 1, backPhaseDuration, value =>
            {
                float f = math.lerp(beginAngle, angle, value);
                pivot.localEulerAngles = new Vector3(0, 0, f);
                
                if(flag)
                {
                    Vector3 v = Vector3.Lerp(beginLocalPos, pivot.position, f);
                    pivot.localPosition = v;
                }
            });
            
            return backPhaseDuration;
        }
    }
}
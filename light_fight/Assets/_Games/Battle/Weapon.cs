using _KITSystem.Utils;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
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

        private void Awake()
        {
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
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Vector3 eulerAngles = pivot.eulerAngles;
                float currentAngle = eulerAngles.z;
                float delta = Mathf.DeltaAngle(currentAngle, angle);
                float endAngle = currentAngle + delta;
                if (tweener != null && tweener.IsPlaying()) tweener.Kill();

                tweener = DOVirtual.Float(currentAngle, endAngle, rotatePhaseDuration, value =>
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
            tweener = DOVirtual.Float(beginAngle, angle, backPhaseDuration,
                value => { pivot.localEulerAngles = new Vector3(0, 0, value); });
            return backPhaseDuration;
        }
    }
}
using System.Collections;
using _KITSystem.EventBus;
using _KITSystem.Utils;
using Animancer;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace _Games.Battle
{
    public class Weapon : MonoBehaviour
    {
        [TitleGroup("Settings")] 
        [SerializeField, Tooltip("Offset tối thiểu của pivot ở góc 90")]
        private Vector3 offsetMin = new Vector3(0, 0.2f, 0);
        [SerializeField, Tooltip("Offset tối đa của pivot ở góc 180")]
        private Vector3 offsetMax = new Vector3(0, 0.5f, 0);
        [SerializeField, Tooltip("Thời gian pivot xoay tới goal")]
        private float rotatePhaseDuration = 0.1f;
        [SerializeField, Tooltip("Thời gian pivot back lại vị trí cũ")]
        private float backPhaseDuration = 0.1f;
        [SerializeField, Tooltip("Thời gian chờ để bắt đầu xoay")]
        private float delayFocus = 0.1f;
        
        [TitleGroup("Feedback")] 
        [SerializeField] private MMF_Player playFeedback;
        [SerializeField] private MMF_Player activeFeedback;
        [SerializeField] private MMF_Player inactiveFeedback;

        [TitleGroup("Element")]
        [SerializeField] private Transform pivot;
        [SerializeField] private SortingGroup sortingGroup; // animator/animation
        
        [TitleGroup("Animation")]
        [SerializeField] private NamedAnimancerComponent animancer;
        [SerializeField] private AnimationClip attackClip;
        
        private float feedbackScaleTime = 1;
        private Vector3 localRotation;
        private Vector3 localPosition;
        private Tweener tweener;
        private Coroutine coroutine;
        private Coroutine animationCoroutine;
        private AnimancerState animancerState;

        private void Awake()
        {
            localRotation = pivot.localEulerAngles;
            localPosition = pivot.localPosition;
        }

        public void Initialize(float timeScale) => feedbackScaleTime = timeScale;

        public void Play() => playFeedback.PlayFeedbacks();

        public float Active()
        {
            sortingGroup.sortingOrder = 1;
            activeFeedback.TimescaleMultiplier = feedbackScaleTime;
            activeFeedback.PlayFeedbacks();
            return activeFeedback.TotalDuration / feedbackScaleTime;
        }

        public void Inactive()
        {
            sortingGroup.sortingOrder = 0;
            inactiveFeedback.TimescaleMultiplier = feedbackScaleTime;
            inactiveFeedback.PlayFeedbacks();
        }

        public void Focus()
        {
            Debug.Log(@"Trục Y rotate bị sai");
          
            void Action(Vector3 goal)
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
                float t = Mathf.Clamp01(Mathf.Abs(delta) / 180f);
                Vector3 offset = Vector3.Lerp(offsetMin, offsetMax, t);
                Vector3 offsetDir = needOffset ? offset : Vector3.zero;
                Vector3 originalLocalPos = pivot.localPosition;
                Vector3 targetLocalPos = originalLocalPos + offsetDir;

                if (tweener != null && tweener.IsActive()) tweener.Complete();
                tweener = DOVirtual.Float(0, 1, rotatePhaseDuration / feedbackScaleTime, value =>
                    {
                        float eased = EaseInOutSine(value);
                        float a = math.lerp(currentAngle, endAngle, eased);
                        pivot.eulerAngles = new Vector3(0, 0, a);

                        if (needOffset)
                        {
                            pivot.localPosition = Vector3.Lerp(originalLocalPos, targetLocalPos, eased);
                        }
                    })
                    .SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
                        animationCoroutine = StartCoroutine(PlayAnimation());
                    });
            }

            void WaitAction()
            {
                Vector3 p = pivot.position;
                float2 position = new float2(p.x, p.y);
                float radius = 5f;
                SystemBus.Publish(new QueryAgentSignal(position, radius, tuple =>
                {
                    int count = tuple.count;
                    float2[] positions = tuple.positions;
                    float2 goal = position;
                    float min = float.MaxValue;
                    for (int i = 0; i < count; i++)
                    {
                        float d = math.distancesq(position, positions[i]);
                        if (d < min)
                        {
                            goal = positions[i];
                            min = d;
                        }
                    }
                    
                    Action(new Vector3(goal.x, goal.y));
                }));
            }
            
            if (delayFocus > 0)
            {
                if (coroutine != null) StopCoroutine(coroutine);
                coroutine = this.WaitInvoke(delayFocus / feedbackScaleTime, WaitAction);
            }
            else
                WaitAction();
        }

        public float StopFocus()
        {
            if (animationCoroutine != null) StopCoroutine(animationCoroutine);
            if (animancerState != null)
            {
                animancerState.Stop();
                animancerState = null;
            }
            
            Vector3 beginLocalPos = pivot.localPosition;
            bool flag = beginLocalPos != localPosition;

            float begin = pivot.localEulerAngles.z;
            float target = localRotation.z;
            float final = begin + Mathf.DeltaAngle(begin, target);

            float d = backPhaseDuration / feedbackScaleTime;
            
            if (tweener != null && tweener.IsPlaying()) tweener.Kill();
            tweener = DOVirtual.Float(0, 1, d, value =>
            {
                Vector3 rot = pivot.localEulerAngles;
                rot.z = Mathf.Lerp(begin, final, value);
                pivot.localEulerAngles = rot;
                
                if (flag)
                {
                    float eased = EaseInOutSine(value);
                    pivot.localPosition = Vector3.Lerp(beginLocalPos, localPosition, eased);
                }
            }).SetEase(Ease.Linear);
            
            return d;
        }
        
        private static float EaseInOutSine(float t)
        {
            return -(math.cos(math.PI * t) - 1) * 0.5f;
        }

        private IEnumerator PlayAnimation()
        {
            float duration = attackClip.length;
            while (true)
            {
                animancerState = animancer.Play(attackClip);
                animancerState.Time = 0;
                animancerState.Speed = feedbackScaleTime;
                yield return new WaitForSeconds(duration / feedbackScaleTime);
                yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));
            }
        }
    }
}
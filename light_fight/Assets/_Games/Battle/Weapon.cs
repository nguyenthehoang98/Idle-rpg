using System;
using System.Collections;
using _KITSystem.EventBus;
using _KITSystem.Grid;
using _KITSystem.SkillSystem.Config;
using _KITSystem.SkillSystem.Runtime;
using _KITSystem.Utils;
using Animancer;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Action = System.Action;

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
        [SerializeField] private Transform muzzle;
        [SerializeField] private Transform zPivot;
        [SerializeField] private Transform xPivot;
        [SerializeField] private SortingGroup sortingGroup; // animator/animation
        
        [TitleGroup("Animation")]
        [SerializeField] private NamedAnimancerComponent animancer;
        [SerializeField] private AnimationClip attackClip;
        
        [TitleGroup("Skills")]
        [SerializeField] private SkillConfig skillConfig;
        [SerializeField] private float recoveryTime = 0.2f;
        [SerializeField] private float scanRadius = 6;

        private float feedbackScaleTime = 1;
        private Vector3 eulerAngles;
        private Vector3 localPosition;
        private Tweener tweener;
        private Coroutine coroutine;
        private Coroutine animationCoroutine;
        private AnimancerState animancerState;

        public void Initialize(int pivotZ, int pivotX, float timeScale)
        {
            feedbackScaleTime = timeScale;
            xPivot.localRotation = Quaternion.Euler(pivotX, 0, 0);
            zPivot.localRotation = Quaternion.Euler(0, 0, pivotZ);
        }

        public void Play()
        {
            eulerAngles = zPivot.eulerAngles;
            localPosition = zPivot.localPosition;
            playFeedback.PlayFeedbacks();
        }

        public float Active()
        {
            sortingGroup.sortingOrder = 1;
            activeFeedback.TimescaleMultiplier = feedbackScaleTime;
            activeFeedback.PlayFeedbacks();
            return activeFeedback.TotalDuration / feedbackScaleTime;
        }

        public void Inactive()
        {
            if (tweener != null && tweener.IsActive()) tweener.Kill();
            if (animationCoroutine != null) StopCoroutine(animationCoroutine);
            if (animancerState != null) animancerState.Stop();
            sortingGroup.sortingOrder = 0;
            inactiveFeedback.TimescaleMultiplier = feedbackScaleTime;
            inactiveFeedback.PlayFeedbacks();
        }

        public void Focus()
        {
            void Action(AgentData agentData)
            {
                // @"Xứ lý tính duration nếu mà góc gần thay quay nhanh hơn. max=rotatePhaseDuration
                Vector3 goal = new Vector3(agentData.position.x, agentData.position.y, 0);
                RotateTo(goal, true, () =>
                {
                    if (animationCoroutine != null) StopCoroutine(animationCoroutine);
                    animationCoroutine = StartCoroutine(PlayAnimation(() =>
                    {
                        CompleteFocus(agentData);
                    }, AutoFocus));
                });
            }

            if (delayFocus > 0)
            {
                if (coroutine != null) StopCoroutine(coroutine);
                coroutine = this.WaitInvoke(delayFocus / feedbackScaleTime, () => ScanNearestAgent(scanRadius, Action));
            }
            else
                ScanNearestAgent(scanRadius, Action);
        }

        private void AutoFocus()
        {
            void Action(AgentData agentData)
            {
                Vector3 goal = new Vector3(agentData.position.x, agentData.position.y, 0);
                RotateTo(goal, false, () =>
                {
                    if (animationCoroutine != null) StopCoroutine(animationCoroutine);
                    animationCoroutine = StartCoroutine(PlayAnimation(() =>
                    {
                        CompleteFocus(agentData);
                    }, AutoFocus));
                });
            }

            ScanNearestAgent(scanRadius, Action);
        }

        private void CompleteFocus(AgentData agentData)
        {
            Vector3 mPos = muzzle.position;
            SkillFactory.Build(new float2(mPos.x, mPos.y), agentData.position, skillConfig);
#if UNITY_EDITOR
            Vector3 newAgentPos = new Vector3(agentData.position.x, agentData.position.y);
            Debug.DrawLine(mPos, newAgentPos, Color.magenta, 0.25f);
#endif 
        }

        public void RotateTo(Vector3 goal, bool needUpdatePosition, Action onComplete)
        {
            Vector3 position = zPivot.position;
            Vector3 direction = goal - position;
            direction.z = 0;
            if (direction.sqrMagnitude < 0.0001f) return;
                
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float currentAngle = zPivot.eulerAngles.z;
            float delta = Mathf.DeltaAngle(currentAngle, targetAngle);
            float endAngle = currentAngle + delta;
            float startSigned = Mathf.DeltaAngle(0f, currentAngle);
            bool lastFlip = Mathf.Abs(startSigned) > 90f;

#if UNITY_EDITOR
            Vector3 v1 = position + (muzzle.position - position).normalized;
            Vector3 v2 = position + direction.normalized;
            Debug.DrawLine(position, v1, Color.yellow, 0.5f);
            Debug.DrawLine(position, v2, Color.yellow, 0.5f);
            Debug.DrawLine(v1, v2, Color.yellow, 0.5f);
#endif

            float t = Mathf.Clamp01(Mathf.Abs(delta) / 180f);
            Vector3 offset = Vector3.Lerp(offsetMin, offsetMax, t);
            Vector3 offsetDir = needUpdatePosition ? offset : Vector3.zero;
            Vector3 originalLocalPos = zPivot.localPosition;
            Vector3 targetLocalPos = originalLocalPos + offsetDir;
            
            if (tweener != null && tweener.IsActive()) tweener.Complete();
            tweener = DOVirtual.Float(0, 1, rotatePhaseDuration / feedbackScaleTime, value =>
                {
                    float a = Mathf.LerpAngle(currentAngle, endAngle, value);
                    zPivot.eulerAngles = new Vector3(0, 0, a);
#if UNITY_EDITOR
                    float rad = a * Mathf.Deg2Rad;
                    Vector3 dir = new Vector3(
                        Mathf.Cos(rad),
                        Mathf.Sin(rad),
                        0);
                    Debug.DrawLine(position, position + dir * 1.5f, Color.red, 0.05f);
#endif
                    
                    float signed = Mathf.DeltaAngle(0f, a);
                    bool b = Mathf.Abs(signed) > 90f;
                    if (b != lastFlip)
                    {
                        lastFlip = b;
                        xPivot.localRotation = Quaternion.Euler(b ? 180: 0, 0, 0);
                    }

                    if (needUpdatePosition)
                    {
                        zPivot.localPosition = Vector3.Lerp(originalLocalPos, targetLocalPos, value);
                    }
                })
                .SetEase(Ease.InOutSine)
                .OnComplete(() => onComplete?.Invoke());
        }

        private void ScanNearestAgent(float radius, Action<AgentData> callback)
        {
            float2 position = float2.zero;
            SystemBus.Publish(new WeaponQueryAgentSignal(position, radius, tuple =>
            {
                int count = tuple.count;
                AgentData[] agents = tuple.agentsData;
                AgentData agent = default(AgentData);
                float min = float.MaxValue;
                for (int i = 0; i < count; i++)
                {
                    AgentData ad = agents[i];
                    if (ad.isDead) continue;
                    
                    float2 pos = ad.position;
                    float d = math.distancesq(pos, position);
                    if (d < min)
                    {
                        agent = ad;
                        min = d;
                    }
                }
                callback?.Invoke(agent);
            }));
        }

        public float StopFocus()
        {
            if (animationCoroutine != null) StopCoroutine(animationCoroutine);
            if (animancerState != null)
            {
                animancerState.Stop();
                animancerState = null;
            }
            
            Vector3 beginLocalPosition = zPivot.localPosition;
            bool needUpdatePosition = beginLocalPosition != localPosition;
            
            float currentAngle = zPivot.eulerAngles.z;
            float delta = Mathf.DeltaAngle(currentAngle, eulerAngles.z);
            float endAngle = currentAngle + delta;
            float startSigned = Mathf.DeltaAngle(0f, currentAngle);
            bool lastFlip = Mathf.Abs(startSigned) > 90f;
            
            float duration = backPhaseDuration / feedbackScaleTime;
            if (tweener != null && tweener.IsPlaying()) tweener.Kill();
            tweener = DOVirtual.Float(0, 1, duration, value =>
            {
                float a = Mathf.LerpAngle(currentAngle, endAngle, value);
                zPivot.eulerAngles = new Vector3(0, 0, a);

                float signed = Mathf.DeltaAngle(0f, a);
                bool b = Mathf.Abs(signed) > 90f;
                if (b != lastFlip)
                {
                    lastFlip = b;
                    xPivot.localRotation = Quaternion.Euler(b ? 180: 0, 0, 0);
                }

                if (needUpdatePosition)
                {
                    zPivot.localPosition = Vector3.Lerp(beginLocalPosition, localPosition, value);
                }
            }).SetEase(Ease.InOutSine);
            
            return duration;
        }
        
        private IEnumerator PlayAnimation(Action onCompleteFocus, Action onNextFocus)
        {
            float duration = attackClip.length;
            animancerState = animancer.Play(attackClip);
            animancerState.Time = 0;
            animancerState.Speed = feedbackScaleTime;
            yield return new WaitForSeconds(duration / feedbackScaleTime);
            onCompleteFocus?.Invoke();
            yield return new WaitForSeconds(recoveryTime);
            onNextFocus?.Invoke();
        }
    }
}
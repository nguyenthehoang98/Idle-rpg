using System;
using _KITSystem.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Games.Battle
{
    public class Cone : MonoBehaviour
    {
        [TitleGroup("Settings")] 
        [SerializeField] private Color defaultColor;
        [SerializeField] private Sprite[] numbers;
        [SerializeField] private int[] defaultAngles = new int[] {180, 180, 0, 0, 0, 0};
        [SerializeField] private Color[] selectedColors = new Color[4];
        
        [TitleGroup("Feedback")] 
        [SerializeField] private MMF_Player zoomOutFeedback;
        [SerializeField] private MMF_Player zoomInFeedback;
        [SerializeField] private MMF_Player initFeedback;
        [SerializeField] private MMF_Player playFeedback;

        [TitleGroup("Elements")] 
        [SerializeField] private Star[] stars;
        [SerializeField] private SpriteRenderer number;
        [SerializeField] private SpriteRenderer background;
        [SerializeField] private Weapon weapon;

        private float feedbackScaleTime = 1;
        private bool isInitialized = false;
        private int currentStack;
        private int maxStar;
        private Tween tween;
        private Coroutine coroutine;
        
        public bool IsPlaying { get; private set; }

        public UniTask Initialize(int order, int maxStar, float timeScale)
        {
            if (isInitialized) 
                return UniTask.CompletedTask;
            this.maxStar = maxStar;
            feedbackScaleTime = timeScale;
            transform.localRotation = Quaternion.Euler(0, 0, -60 * order);
            initFeedback.TimescaleMultiplier = feedbackScaleTime;
            initFeedback.PlayFeedbacks();
            isInitialized = true;
            
            number.sprite = numbers[order];

            int angle = defaultAngles[order];
            weapon.Initialize(timeScale);
            weapon.transform.localRotation = Quaternion.Euler(0, angle, 0);
            
            return UniTask.WaitForSeconds(initFeedback.TotalDuration / feedbackScaleTime);
        }

        public float Play()
        {
            playFeedback.TimescaleMultiplier = feedbackScaleTime;
            playFeedback.PlayFeedbacks();
            
            for (int i = 0; i < maxStar; i++)
            {
                int index = i;
                float delay = index * 0.1f;
                this.WaitInvoke(delay, () =>
                {
                    stars[index].Play();
                });
            }
            
            float f = playFeedback.TotalDuration / feedbackScaleTime;
            this.WaitInvoke(f, () =>
            {
                weapon.gameObject.SetActive(true);
                weapon.Play();
            });
            return f;
        }

        public void Active()
        {
            IsPlaying = true;
            
            if (selectedColors.Length == 0)
            {
                Debug.LogError("You must select at least one color");
                return;
            }
            
            zoomOutFeedback.PlayerCompleteFeedbacks();
            zoomInFeedback.TimescaleMultiplier = feedbackScaleTime;
            zoomInFeedback.PlayFeedbacks();
            
            if (coroutine != null) StopCoroutine(coroutine);
            float f = weapon.Active();
            float duration = zoomInFeedback.TotalDuration / feedbackScaleTime;
            coroutine = this.WaitInvoke(Mathf.Max(f, duration) + 0.1f, () =>
            {
                weapon.Focus();
            });
        }

        public void StackColor(int stack)
        {
            if (currentStack == stack)
            {
                stars[stack - 1].Active();
                return;
            }

            if (stack - currentStack > 0)
            {
                for (int i = currentStack; i < stack; i++)
                {
                    Star star = stars[i];
                    this.WaitInvoke(star.DelayActive * i, () =>
                    {
                        star.Active();
                    });
                }
            }
            else
            {
                int d = currentStack - stack;
                for (int i = currentStack; i > stack; i--)
                {
                    Star star = stars[i];
                    this.WaitInvoke(star.DelayActive * (d - i), () =>
                    {
                        star.Inactive();
                    });
                }
            }
            
            currentStack = stack;
            
            Color color = defaultColor;
            if (stack <= selectedColors.Length) 
                color = selectedColors[stack - 1];
            float duration = zoomInFeedback.TotalDuration / feedbackScaleTime;
            
            if (tween != null && tween.IsPlaying()) tween.Complete();
            tween = background.DOColor(color, duration)
                .SetEase(Ease.Linear);
        }
        
        public void Inactive()
        {
            IsPlaying = false;
            int prevStack = currentStack;
            currentStack = 0;
            
            Action action = () =>
            {
                weapon.Inactive();

                zoomInFeedback.PlayerCompleteFeedbacks();
                zoomOutFeedback.TimescaleMultiplier = feedbackScaleTime;
                zoomOutFeedback.PlayFeedbacks();
                
                float duration = zoomOutFeedback.TotalDuration / feedbackScaleTime;
                if (tween != null && tween.IsPlaying()) tween.Complete();
                tween = background.DOColor(defaultColor, duration)
                    .SetEase(Ease.InCubic);
            };

            for (int i = prevStack - 1; i >= 0; i--)
            {
                Star star = stars[i];
                this.WaitInvoke(star.DelayInactive * (prevStack - 1 - i), () =>
                {
                    star.Inactive();
                });
            }
            
            float f = weapon.StopFocus();
            if (f > 0)
            {
                if (coroutine != null) StopCoroutine(coroutine);
                coroutine = this.WaitInvoke(f, action);
            }
            else action();
        }

        public Vector3 GetStarPosition(int index) => stars[index].transform.position;
        public Vector3 GetStarRotation(int index) => stars[index].transform.eulerAngles;
    }
}
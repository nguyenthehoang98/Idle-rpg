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
        [SerializeField] private int[] zPivots = new int[] { 90, 90, 90, 90, 90, 90 };
        [SerializeField] private int[] xPivots = new int[] { 0, 0, 0, 180, 180, 180 };
        [SerializeField] private Color[] selectedColors = new Color[4];
        
        [TitleGroup("Feedback")] 
        [SerializeField] private MMF_Player zoomOutFeedback;
        [SerializeField] private MMF_Player zoomInFeedback;
        [SerializeField] private MMF_Player initFeedback;
        [SerializeField] private MMF_Player playFeedback;

        [TitleGroup("Elements")] 
        [SerializeField] private Star[] stars;
        [SerializeField] private SpriteRenderer highlight;
        [SerializeField] private Weapon weapon;
        
        [TitleGroup("Debug")]
        [SerializeField] private float shineWidth;
        [SerializeField] private Color shineColor;
        
        private float feedbackScaleTime = 1;
        private bool isInitialized = false;
        private int currentStack;
        private int maxStar;
        private Tween tween;
        private Coroutine coroutine;
        private MaterialPropertyBlock colorProperty;
        private MaterialPropertyBlock widthProperty;
        
        public bool IsPlaying { get; private set; }

        public Weapon Weapon => weapon;
        
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
            weapon.Initialize(zPivots[order], xPivots[order], timeScale);
#if UNITY_EDITOR
            weapon.name = "Weapon " + order;
#endif
            return UniTask.WaitForSeconds(0.075f);
        }

        private void Awake()
        {
            colorProperty = new MaterialPropertyBlock();
            widthProperty = new MaterialPropertyBlock();
        }

        public float Play()
        {
            playFeedback.TimescaleMultiplier = feedbackScaleTime;
            for (int i = 0; i < maxStar; i++)
            {
                int index = i;
                float delay = index * 0.1f;
                this.WaitInvoke(delay, () => { stars[index].Play(); });
            }

            float d1 = 0.1f * maxStar + 1f;
            float d2 = 0.1f * maxStar + 1.5f;
            this.WaitInvoke(d1, weapon.Play);
            this.WaitInvoke(d2, playFeedback.PlayFeedbacks);
    
            return d2 + playFeedback.TotalDuration / feedbackScaleTime;
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

        public void ResetStack()
        {
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].Inactive();
            }
        }

        public void DoStack(int stack)
        {
            int prevStack = currentStack;
            
            currentStack = stack;
            stars[stack - 1].Active();
            
            // từ đây đổ xuống thì hoạt động đúng
            Color color = Color.white;
            
            if (stack <= selectedColors.Length) 
                color = selectedColors[stack - 1];
            
            float duration = 0.3f / feedbackScaleTime;
            
            if (tween != null && tween.IsPlaying()) tween.Complete();
            if (stack == 1 && prevStack == 0)
            {
                SetColor(color);
                SetWidth(0.05f);
                tween = DOVirtual.Float(0.05f, 1, duration, SetWidth);
            }
            else
            {
                float width = shineWidth;
                Color currentColor = shineColor;
                tween = DOVirtual.Float(0.05f, 1, duration, value =>
                {
                    if (value > width) SetWidth(value);
                    SetColor(Color.Lerp(currentColor, color, value));
                });
            }
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
                tween = DOVirtual.Float(1f, 0.05f, duration, SetWidth);
            };

            int order = 0;
            for (int i = prevStack - 1; i >= 0; i--)
            {
                Star star = stars[i];
                this.WaitInvoke(star.DelayInactive * order, () =>
                {
                    star.Inactive();
                });

                order++;
            }
            
            float f = weapon.StopFocus();
            if (f > 0)
            {
                if (coroutine != null) StopCoroutine(coroutine);
                coroutine = this.WaitInvoke(f, action);
            }
            else action();
        }

        private void SetColor(Color color)
        {
            shineColor = color;
            highlight.GetPropertyBlock(colorProperty);
            colorProperty.SetColor("_ShineColor", color);
            highlight.SetPropertyBlock(colorProperty);
        }

        private void SetWidth(float width)
        {
            shineWidth = width;
            highlight.GetPropertyBlock(widthProperty);
            widthProperty.SetFloat("_ShineWidth", width);
            highlight.SetPropertyBlock(widthProperty);
        }

        public Vector3 GetStarPosition(int index) => stars[index].transform.position;
        public Vector3 GetStarRotation(int index) => stars[index].transform.eulerAngles;
    }
}
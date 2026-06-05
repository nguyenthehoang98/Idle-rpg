using System;
using _KITSystem.Utils;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _FightCode.Battle.View
{
    public class SlotView : MonoBehaviour
    {
        [TitleGroup("Settings")]
        [SerializeField] private Color[] selectedColors = new Color[4];

        [TitleGroup("Elements")]
        [SerializeField] private SpriteRenderer highlight;
        [SerializeField] private Transform pivot;
        [SerializeField] private StarView[] stars;

        [TitleGroup("Feedback")]
        [SerializeField] private AnimationCurve deactivateLerpCurve;
        [SerializeField] private AnimationCurve activateLerpCurve;
        [SerializeField] private MMF_Player initFeedback;
        [SerializeField] private MMF_Player playFeedback;
        [SerializeField] private MMF_Player deactivateFeedback;
        [SerializeField] private MMF_Player activateFeedback;
        [TitleGroup("Debug")]
        [SerializeField] private float shineWidth;
        [SerializeField] private Color shineColor;

        private MaterialPropertyBlock colorProperty;
        private MaterialPropertyBlock widthProperty;
        private int currentStack;
        private Coroutine coroutineLerp;
        private Coroutine coroutineDelayCallback;

        private Vector3 localEulerAngles;

        private void Awake()
        {
            pivot.gameObject.SetActive(false);
            colorProperty = new MaterialPropertyBlock();
            widthProperty = new MaterialPropertyBlock();
        }

        public SlotView Instantiate(Transform parent, Vector3 localEulerAngles)
        {
            SlotView osv = Instantiate(this, parent);
            osv.transform.localEulerAngles = osv.localEulerAngles = localEulerAngles;
            return osv;
        }

        public void Initialize(float timeScale)
        {
            initFeedback.TimescaleMultiplier = timeScale;
            initFeedback.PlayFeedbacks();
        }

        public float Play(float timeScale)
        {
            float d = 0;
            for (int i = 0; i < stars.Length; i++)
            {
                int index = i;
                d = Mathf.Max(d, index * 0.1f / timeScale);
                this.WaitInvoke(d, () => stars[index].Play(timeScale));
            }

            playFeedback.TimescaleMultiplier = timeScale;

            float d1 = d + 0.5f / timeScale;
            this.WaitInvoke(d1, playFeedback.PlayFeedbacks);

            return d1 + playFeedback.TotalDuration / timeScale;
        }

        public void Tick(float deltaTime)
        {
        }

        public void Activate(float delayActivate, float timeScale)
        {
            activateFeedback.TimescaleMultiplier = timeScale;
            
            this.WaitInvoke(delayActivate / timeScale, activateFeedback.PlayFeedbacks);
        }

        public void Deactivate(float delayDeactivate, float timeScale)
        {
            int prevStack = currentStack;

            currentStack = 0;

            Action action = () =>
            {
                for (int i = prevStack - 1; i >= 0; i--)
                {
                    stars[i].Inactive(timeScale);
                }
                
                activateFeedback.PlayerCompleteFeedbacks();

                deactivateFeedback.TimescaleMultiplier = timeScale;
                deactivateFeedback.PlayFeedbacks();

                float duration = deactivateFeedback.TotalDuration / timeScale;
                if (coroutineLerp != null) StopCoroutine(coroutineLerp);
                coroutineLerp = this.CurveNormalize(1f, 0.05f, deactivateLerpCurve, duration, SetWidth);
            };

            if (coroutineDelayCallback != null) StopCoroutine(coroutineDelayCallback);
            coroutineDelayCallback = this.WaitInvoke(delayDeactivate / timeScale, action);
        }

        // totalStack:begin 1
        public void Stack(int totalStack, float lerpDuration, float timeScale)
        {
            int prevStack = currentStack;

            currentStack = totalStack;

            // pre=1:current=2; => Tăng lên
            if (currentStack < prevStack)
            {
                for (int i = currentStack + 1; i <= prevStack; i++)
                {
                    stars[i - 1].Inactive(timeScale);                    
                }
            }
            
            stars[currentStack - 1].Active(timeScale);

            Color color = Color.white;

            if (totalStack <= selectedColors.Length) color = selectedColors[totalStack - 1];

            float duration = lerpDuration / timeScale;

            if (coroutineLerp != null) StopCoroutine(coroutineLerp);

            if (totalStack == 1 && prevStack == 0)
            {
                SetColor(color);
                SetWidth(0.05f);
                coroutineLerp = this.CurveNormalize(0.05f, 1f, activateLerpCurve, duration, SetWidth);
            }
            else
            {
                float width = shineWidth;
                Color currentColor = shineColor;
                coroutineLerp = this.CurveNormalize(0.05f, 1f, activateLerpCurve, duration, value =>
                {
                    if (value > width) SetWidth(value);
                    SetColor(Color.Lerp(currentColor, color, value));
                });
            }
        }

        public Transform WeaponRoot => pivot;

        public Vector3 WorldPosition(int stack)
        {
            return stars[stack - 1].transform.position;
        }

        public Vector3 WorldEulerAngles(int stack)
        {
            return localEulerAngles;
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
    }
}
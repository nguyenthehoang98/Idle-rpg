using System;
using System.Collections;
using _KITSystem.Utils;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Games.Battle.View
{
    public class ObjectSlotView : MonoBehaviour, ISlotView
    {
        [TitleGroup("Settings")] 
        [SerializeField] private Color[] selectedColors = new Color[4];
        [TitleGroup("Elements")] 
        [SerializeField] private SpriteRenderer highlight;
        [SerializeField] private Transform pivot;
        [SerializeField] private StarView[] stars;
        [TitleGroup("Feedback")] 
        [SerializeField] private MMF_Player initFeedback;
        [SerializeField] private MMF_Player playFeedback;
        [SerializeField] private MMF_Player zoomOutFeedback;
        [SerializeField] private MMF_Player zoomInFeedback;
        [TitleGroup("Debug")]
        [SerializeField] private float shineWidth;
        [SerializeField] private Color shineColor;

        private Vector3 localEulerAngles;
        
        private MaterialPropertyBlock colorProperty;
        private MaterialPropertyBlock widthProperty;
        private int currentStack;
        private Coroutine coroutineLerp;
        private Coroutine coroutineDelayCallback;

        private void Awake()
        {
            pivot.gameObject.SetActive(false);
            colorProperty = new MaterialPropertyBlock();
            widthProperty = new MaterialPropertyBlock();
        }

        public ISlotView Instantiate(Transform parent, Vector3 localEulerAngles)
        {
            this.localEulerAngles = localEulerAngles;
            var view = Instantiate(transform, parent);
            view.localEulerAngles = localEulerAngles;
            return view.GetComponent<ISlotView>();
        }

        public float Initialize(float timeScale)
        {
            initFeedback.TimescaleMultiplier = timeScale;
            initFeedback.PlayFeedbacks();
            return initFeedback.TotalDuration / timeScale;
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

            //float d1 = d + 0.5f / timeScale;
            float d2 = d + 0.5f / timeScale;
            //this.WaitInvoke(d1, () => { Debug.Log("weapon_active"); });
            this.WaitInvoke(d2, playFeedback.PlayFeedbacks);

            return d2 + playFeedback.TotalDuration / timeScale;
        }

        public float Activate(float timeScale)
        {
            zoomInFeedback.TimescaleMultiplier = timeScale;
            zoomInFeedback.PlayFeedbacks();
            return zoomInFeedback.TotalDuration / timeScale;
        }

        public float Deactivate(float timeScale)
        {
            int prevStack = currentStack;
            
            currentStack = 0;

            Action action = () =>
            {
                zoomInFeedback.PlayerCompleteFeedbacks();

                zoomOutFeedback.TimescaleMultiplier = timeScale;
                zoomOutFeedback.PlayFeedbacks();

                float duration = zoomOutFeedback.TotalDuration / timeScale;
                if (coroutineLerp != null) StopCoroutine(coroutineLerp);
                coroutineLerp = this.LerpNormalize(1f, 0.05f, duration, SetWidth);
            };

            int order = 0;
            for (int i = prevStack - 1; i >= 0; i--)
            {
                StarView star = stars[i];
                this.WaitInvoke(0.2f * order, () => { star.Inactive(timeScale); });

                order++;
            }

            float weaponRollbackDuration = 0;
            if (weaponRollbackDuration > 0)
            {
                if (coroutineDelayCallback != null) StopCoroutine(coroutineDelayCallback);
                coroutineDelayCallback = this.WaitInvoke(weaponRollbackDuration, action);
            }
            else action();

            return 0;
        }

        public void Stack(int stack, float timeScale)
        {
            int prevStack = currentStack;

            currentStack = stack;

            if (prevStack == currentStack)
            {
                stars[currentStack - 1].Active(timeScale);
                return;
            }
            else
            {
                for (int i = 0; i < stars.Length; i++)
                {
                    stars[i].Inactive(timeScale);
                }

                for (int i = 0; i < currentStack; i++)
                {
                    int index = i;
                    this.WaitInvoke(0.1f * i, () => { stars[index].Active(timeScale); });
                }
            }

            Color color = Color.white;

            if (stack <= selectedColors.Length) color = selectedColors[stack - 1];

            float duration = 0.5f / timeScale;

            if (coroutineLerp != null) StopCoroutine(coroutineLerp);

            if (stack == 1 && prevStack == 0)
            {
                SetColor(color);
                SetWidth(0.05f);
                coroutineLerp = this.LerpNormalize(0.05f, 1f, duration, SetWidth);
            }
            else
            {
                float width = shineWidth;
                Color currentColor = shineColor;
                coroutineLerp = this.LerpNormalize(0.05f, 1f, duration, value =>
                {
                    if (value > width) SetWidth(value);
                    SetColor(Color.Lerp(currentColor, color, value));
                });
            }
        }

        public Vector3 WorldPosition => stars[0].transform.position;
        public Vector3 WorldEulerAngles => localEulerAngles;

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
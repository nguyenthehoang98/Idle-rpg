using System;
using System.Collections.Generic;
using _KITSystem.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Games.Battle
{
    public class Cone : MonoBehaviour
    {
        [TitleGroup("Settings")] 
        [SerializeField] private Color defaultColor;
        [SerializeField] private int[] defaultAngles = new int[] {180, 180, 0, 0, 0, 0};
        [SerializeField] private Color[] selectedColors = new Color[4];
        
        [TitleGroup("Feedback")] 
        [SerializeField] private MMF_Player zoomOutFeedback;
        [SerializeField] private MMF_Player zoomInFeedback;
        [SerializeField] private MMF_Player initFeedback;
        [SerializeField] private MMF_Player playFeedback;

        [TitleGroup("Elements")]
        [SerializeField] private SpriteRenderer background;
        [SerializeField] private Weapon weapon;

        private float feedbackScaleTime = 1;
        private bool isInitialized = false;
        private List<int> dicesId = new List<int>();
        private Tween tween;

        public UniTask Initialize(int order, float timeScale)
        {
            if (isInitialized) 
                return UniTask.CompletedTask;
            feedbackScaleTime = timeScale;
            transform.localRotation = Quaternion.Euler(0, 0, -60 * order);
            initFeedback.TimescaleMultiplier = feedbackScaleTime;
            initFeedback.PlayFeedbacks();
            isInitialized = true;

            int angle = defaultAngles[order - 1];
            weapon.Initialize(timeScale);
            weapon.transform.localRotation = Quaternion.Euler(0, angle, 0);
            
            return UniTask.WaitForSeconds(initFeedback.TotalDuration / feedbackScaleTime);
        }

        public float Play()
        {
            playFeedback.TimescaleMultiplier = feedbackScaleTime;
            playFeedback.PlayFeedbacks();
            return playFeedback.TotalDuration / feedbackScaleTime;
        }

        public void Active()
        {
            if (selectedColors.Length == 0)
            {
                Debug.LogError("You must select at least one color");
                return;
            }
            
            zoomOutFeedback.PlayerCompleteFeedbacks();
            zoomInFeedback.TimescaleMultiplier = feedbackScaleTime;
            zoomInFeedback.PlayFeedbacks();
          
            Color color = selectedColors[0];
            float duration = zoomInFeedback.TotalDuration / feedbackScaleTime;
            if (tween != null && tween.IsPlaying()) tween.Complete();
            tween = background.DOColor(color, duration)
                .SetEase(Ease.OutCubic);
            
            float f = weapon.Active();
            this.WaitInvoke(Mathf.Max(f, duration), () =>
            {
                weapon.Focus(new Vector3(Random.value, Random.value));
            });
        }

        public void SetMultiplierColor()
        {
            if (selectedColors.Length == 0)
            {
                Debug.LogError("You must select at least one color");
                return;
            }
            
            int stack = dicesId.Count;
            Color color = defaultColor;
            if (stack <= selectedColors.Length) 
                color = selectedColors[stack - 1];
            float duration = zoomInFeedback.TotalDuration / feedbackScaleTime;
            
            if (tween != null && tween.IsPlaying()) tween.Kill();
            tween = background.DOColor(color, duration * 0.5f)
                .SetEase(Ease.OutCubic);
        }

        public void Inactive()
        {
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
            
            float f = weapon.StopFocus();

            if (f > 0)
                this.WaitInvoke(f, action);
            else action();
        }
        
        public void InsertId(int dice) => dicesId.Add(dice);
        
        public int Stack => dicesId.Count;
        
        public void RemoveId(int dice) => dicesId.Remove(dice);
    }
}
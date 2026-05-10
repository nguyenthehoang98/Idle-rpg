using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Games.Battle
{
    public class Cone : MonoBehaviour
    {
        [TitleGroup("Feedback")] 
        [SerializeField] private MMF_Player zoomOutFeedback;
        [SerializeField] private MMF_Player zoomInFeedback;
        [SerializeField] private MMF_Player initFeedback;
        [SerializeField] private MMF_Player playFeedback;

        [TitleGroup("Elements")]
        [SerializeField] private SpriteRenderer background;
        [SerializeField] private Weapon weapon;
        [SerializeField] private Color defaultColor;
        [SerializeField] private Color selectedColor;
        [SerializeField] private Color selectedX2Color;
        [SerializeField] private Color selectedX3Color;

        private bool isInitialized = false;
        private List<int> dicesId = new List<int>();
        private Tween tween;

        public UniTask Initialize(int order)
        {
            if (isInitialized) 
                return UniTask.CompletedTask;
            transform.localRotation = Quaternion.Euler(0, 0, -60 * order);
            initFeedback.PlayFeedbacks();
            isInitialized = true;
            return UniTask.WaitForSeconds(initFeedback.TotalDuration);
        }

        public float Play()
        {
            playFeedback.PlayFeedbacks();
            return playFeedback.TotalDuration;
        }

        public void Active()
        {
            Color color = selectedColor;
            zoomOutFeedback.PlayerCompleteFeedbacks();
            zoomInFeedback.PlayFeedbacks();
            float duration = zoomInFeedback.TotalDuration;
            if (tween != null && tween.IsPlaying()) tween.Complete();
            tween = background.DOColor(color, duration)
                .SetEase(Ease.OutCubic);
            weapon.Active();
        }

        public void SetMultiplierColor()
        {
            int stack = dicesId.Count;
            Color color = selectedColor;
            if (stack == 2) color = selectedX2Color;
            else if (stack == 3) color = selectedX3Color;
            float duration = zoomInFeedback.TotalDuration;
            if (tween != null && tween.IsPlaying()) tween.Complete();
            tween = background.DOColor(color, duration * 0.5f)
                .SetEase(Ease.OutCubic);
        }

        public void Inactive()
        { 
            Color color = defaultColor;
            zoomInFeedback.PlayerCompleteFeedbacks();
            zoomOutFeedback.PlayFeedbacks();
            float duration = zoomOutFeedback.TotalDuration;
            if (tween != null && tween.IsPlaying()) tween.Complete();
            tween = background.DOColor(color, duration)
                .SetEase(Ease.InCubic);
            weapon.Inactive();
        }
        
        public void InsertId(int dice) => dicesId.Add(dice);
        
        public int Stack => dicesId.Count;
        
        public void RemoveId(int dice) => dicesId.Remove(dice);
    }
}
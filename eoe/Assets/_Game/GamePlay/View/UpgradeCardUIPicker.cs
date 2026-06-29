using System;
using System.Collections.Generic;
using _Game.Configs;
using LitMotion.Animation;
using UnityEngine;

namespace _Game.GamePlay.View
{
    public class UpgradeCardUIPicker : MonoBehaviour
    {
        [SerializeField] private CardItem[] cardItems;
        [SerializeField] private LitMotionAnimation openAnimation;
        [SerializeField] private LitMotionAnimation closeAnimation;
        
        public event Action<WeaponUpgradeData> OnPickCard;

        private void Awake()
        {
            for (int i = 0; i < cardItems.Length; i++)
            {
                cardItems[i].OnSelected += data =>
                {
                    OnPickCard?.Invoke(data);
                };
            }
        }

        public void Show(List<WeaponUpgradeData> list)
        {
            for (int i = 0; i < cardItems.Length; i++)
            {
                cardItems[i].Init(list[i]);
            }
            
            openAnimation.Play();
        }

        public void Hide()
        {
            closeAnimation.Play();
        }
    }
}
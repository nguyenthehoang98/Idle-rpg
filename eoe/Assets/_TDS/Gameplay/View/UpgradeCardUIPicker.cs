using System;
using System.Collections.Generic;
using _TDS.GameConfig;
using Cysharp.Threading.Tasks;
using LitMotion.Animation;
using UnityEngine;

namespace _TDS.Gameplay.View
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

        public async UniTask Show(List<CardItemData> list)
        {
            for (int i = 0; i < cardItems.Length; i++)
            {
                await cardItems[i].Init(list[i]);
            }
            
            openAnimation.Play();
        }

        public void Hide()
        {
            closeAnimation.Play();
        }
    }
}
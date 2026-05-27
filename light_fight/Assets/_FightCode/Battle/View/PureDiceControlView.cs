using System;
using _FightCode.Battle.Model;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _FightCode.Battle.View
{
    public class PureDiceControlView : IDiceControlView
    {
        public event Action OnInitialized;
        public event Action<float> OnSpeedChanged;

        async void AutoInitialize()
        {
            await UniTask.DelayFrame(1);
            OnInitialized?.Invoke();
        }
      
        public IDiceControlView Instantiate(Transform parent)
        {
            PureDiceControlView view = new PureDiceControlView();
            view.AutoInitialize();
            return view;
        }

        public void Initialize(BattleShare share, BattleSetting setting)
        {
        }
    }
}
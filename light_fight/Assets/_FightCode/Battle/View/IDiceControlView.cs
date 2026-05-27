using System;
using _FightCode.Battle.Model;
using UnityEngine;

namespace _FightCode.Battle.View
{
    public interface IDiceControlView
    {
        event Action OnInitialized;
        event Action<float> OnSpeedChanged;
        IDiceControlView Instantiate(Transform parent);
        void Initialize(BattleShare share, BattleSetting setting);
    }
}
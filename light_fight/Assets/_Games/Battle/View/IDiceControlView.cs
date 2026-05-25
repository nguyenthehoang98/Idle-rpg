using System;
using _Games.Battle.Model;
using UnityEngine;

namespace _Games.Battle.View
{
    public interface IDiceControlView
    {
        event Action OnInitialized;
        event Action<float> OnSpeedChanged;
        IDiceControlView Instantiate(Transform parent);
        void Initialize(BattleShare share, BattleSetting setting);
    }
}
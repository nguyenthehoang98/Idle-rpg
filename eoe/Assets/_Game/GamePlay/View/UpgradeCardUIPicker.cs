using System;
using _Game.Configs;
using UnityEngine;

namespace _Game.GamePlay.View
{
    public class UpgradeCardUIPicker : MonoBehaviour
    {
        public event Action<WeaponUpgradeData> OnPickCard;

        public void Show()
        {
        }

        public void Close()
        {
        }
    }
}
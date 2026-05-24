using System;
using UnityEngine;

namespace _Games.Battle.View
{
    public interface IDiceView
    {
        void ShowValue(int value);
        void SetLocked(bool locked);
        void SetCooldownProgress(float progress);
        void PlayRoll(int value, Action onComplete);
        void SetColor(Color color);
        void SetScaleTime(float scaleTime);
        RectTransform RectTransform { get; }
    }
}

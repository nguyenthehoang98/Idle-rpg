using Unity.Mathematics;
using UnityEngine;

namespace _Games.Battle.View
{
    public interface IDiceView
    {
        IDiceView Instantiate();
        void Initialize(Transform parent);
        void SetLocked(bool locked);
        void SetValue(int value);
        void SetProgress(float progress);
        void SetColor(Color color);
        float Roll(int value);
        
        float2 RectTransformSize { get; }
    }
}

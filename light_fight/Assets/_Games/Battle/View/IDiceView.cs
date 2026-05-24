using Unity.Mathematics;
using UnityEngine;

namespace _Games.Battle.View
{
    public interface IDiceView
    {
        IDiceView Instantiate(Transform parent);
        void Initialize(float cooldown);
        void SetLocked(bool locked);
        void SetValue(int value);
        
        float2 RectTransformSize { get; }
    }
}

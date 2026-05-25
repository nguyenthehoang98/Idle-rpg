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
        void SetSpeed(float speed);
        float Roll(int value);
        
        Vector3 WorldPosition { get; }
        float2 RectTransformSize { get; }
    }
}

using UnityEngine;

namespace _Games.Battle.View
{
    public interface ISlotView
    {
        ISlotView Instantiate(Transform parent, Vector3 localEulerAngles);
        void Initialize(float timeScale);
        float Play(float timeScale);
        void Activate(float delayActivate, float timeScale);
        void Deactivate(float delayDeactivate, float timeScale);
        void Stack(int totalStack, float lerpDuration, float timeScale);

        Vector3 WorldPosition(int stack);
        Vector3 WorldEulerAngles(int stack);
    }
}
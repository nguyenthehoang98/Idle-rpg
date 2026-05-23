using UnityEngine;

namespace _Games.Battle.View
{
    public interface ISlotView
    {
        ISlotView Instantiate(Transform parent, Vector3 localEulerAngles);
        float Initialize(float timeScale);
        float Play(float timeScale);
        float Activate(float timeScale);
        float Deactivate(float timeScale);
    }
}
using Unity.Mathematics;
using UnityEngine;

namespace _FightCode.Battle.View
{
    public class PureDiceView : IDiceView
    {
        public IDiceView Instantiate()
        {
            return new PureDiceView();
        }

        public void Initialize(Transform parent)
        {

        }

        public void SetLocked(bool locked)
        {

        }

        public void SetValue(int value)
        {

        }

        public void SetProgress(float progress)
        {

        }

        public void SetColor(Color color)
        {

        }

        public void SetSpeed(float speed)
        {

        }

        public float Roll(int value)
        {
            return 0;
        }

        public Vector3 WorldPosition => Vector3.zero;
        public float2 RectTransformSize => float2.zero;
    }
}
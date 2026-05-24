using Unity.Mathematics;
using UnityEngine;

namespace _Games.Battle.View
{
    public class ObjectDiceView : MonoBehaviour, IDiceView
    {
        public IDiceView Instantiate(Transform parent)
        {
            var view = Instantiate(transform, parent);
            view.SetAsLastSibling();
            return view.GetComponent<IDiceView>();
        }

        public void Initialize(float cooldown)
        {
            
        }

        public void SetLocked(bool locked)
        {
            
        }

        public void SetValue(int value)
        {
            
        }

        public float2 RectTransformSize => GetComponent<RectTransform>().sizeDelta;
    }
}
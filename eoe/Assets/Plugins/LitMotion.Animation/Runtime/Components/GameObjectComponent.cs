using System;
using LitMotion.Adapters;
using UnityEngine;
using UnityEngine.UI;

namespace LitMotion.Animation.Components
{
    [Serializable]
    [LitMotionAnimationComponentMenu("Object/Active")]
    public class GameObjectActiveAnimation : BooleanPropertyAnimationComponent<GameObject>
    {
        protected override bool GetValue(GameObject target) =>  target.activeSelf;

        protected override void SetValue(GameObject target, in bool value) => target.SetActive(value);
    }
}
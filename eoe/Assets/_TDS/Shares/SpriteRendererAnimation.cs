using _TDS.Gameplay.View;
using UnityEngine;

namespace _TDS.Battle
{
    public class SpriteRendererAnimation : FrameAnimation<SpriteRenderer>
    {
        protected override void OnUpdateFrame(Sprite sprite)
        {
            reference.sprite = sprite;
        }

        protected override bool Enable
        {
            set => reference.enabled = value;
        }
    }
}
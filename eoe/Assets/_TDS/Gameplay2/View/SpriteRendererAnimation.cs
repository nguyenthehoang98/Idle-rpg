using UnityEngine;

namespace _TDS.Gameplay.View
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
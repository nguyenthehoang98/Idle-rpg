using UnityEngine;

namespace _Game.GamePlay.View
{
    public class SpriteRendererAnimation : BaseAnimation<SpriteRenderer>
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
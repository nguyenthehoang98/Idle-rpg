using UnityEngine;
using UnityEngine.UI;

namespace _Game.GamePlay.View
{
    public class ImageAnimation : BaseAnimation<Image>
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
using UnityEngine;
using UnityEngine.UI;

namespace _TDS.Gameplay.View
{
    public class ImageAnimation : FrameAnimation<Image>
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
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.GamePlay
{
    public class ElementTriggerCellView : EnhancedScrollerCellView
    {
        [SerializeField] private Image img;

        public void SetData(Color color)
        {
            img.color = color;
        }
    }
}
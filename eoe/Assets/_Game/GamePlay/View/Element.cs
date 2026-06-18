using UnityEngine;
using UnityEngine.UI;

namespace _Game.GamePlay.View
{
    public class Element : MonoBehaviour
    {
        [SerializeField] private Image imgOutline;
        [SerializeField] private Image imgCore;

        public void Execute(Color coreColor, Color outlineColor)
        {
            imgOutline.color = outlineColor;
            imgCore.color = coreColor;
        }
    }
}
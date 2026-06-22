using UnityEngine;
using UnityEngine.UI;

namespace _Game.GamePlay.View
{
    public class Element : ObjectAttractor
    {
        [SerializeField] private Image imgCore;

        public void Inactive()
        {
            imgCore.gameObject.SetActive(false);
        }

        public void Active(Color coreColor)
        {
            imgCore.gameObject.SetActive(true);
            
            imgCore.color = coreColor;
        }
    }
}
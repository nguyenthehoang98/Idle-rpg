using UnityEngine;
using UnityEngine.UI;

namespace _Game.GamePlay.View
{
    public class Element : MonoBehaviour
    {
        [SerializeField] private Image imgCore;

        private void Awake()
        {
            Inactive();
        }

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
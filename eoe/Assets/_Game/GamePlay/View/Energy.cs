using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.GamePlay.View
{
    public class Energy : MonoBehaviour
    {
        [SerializeField] private Image imgProgress;
        [SerializeField] private float duration = 1;

        private float elapsedTime = 0;
        
        public event Action OnFill;

        private void Awake()
        {
            imgProgress.fillAmount = 0;            
        }

        void Update()
        {
            elapsedTime += Time.deltaTime;
            
            float f = Mathf.Clamp01(elapsedTime / duration);

            imgProgress.fillAmount = f;
            
            if (f >= 1)
            {
                OnFill?.Invoke();
                elapsedTime = 0;
            }
        }
    }
}
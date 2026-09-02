using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.GamePlay.View
{
    public class Energy : MonoBehaviour, IViewTick
    {
        [SerializeField] private Image imgProgress;
        [SerializeField] private float duration = 1;

        private float elapsedTime = 0;
        private bool isPaused;
        private bool isAnimating;
        
        public event Action OnFill;

        public Task Initialize()
        {
            imgProgress.fillAmount = 0;   
            return Task.CompletedTask;
        }
        
        public void SetAnimating(bool value) => this.isAnimating = value;
        
        public void SetPause(bool value) => this.isPaused = value;

        private void Update() => Tick(Time.deltaTime);

        public void Tick(float deltaTime)
        {
            if (isPaused || isAnimating) return;
            
            elapsedTime += deltaTime;
            
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
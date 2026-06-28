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
        
        public event Action OnFill;

        public Task Initialize()
        {
            imgProgress.fillAmount = 0;   
          
            return Task.CompletedTask;
        }
        
        public void SetPause(bool pause) => this.isPaused = pause;

        private void Update() => Tick(Time.deltaTime);

        public void Tick(float deltaTime)
        {
            if (isPaused) return;
            
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
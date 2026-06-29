using System;
using UnityEngine;

namespace _Game.GamePlay.View
{
    public abstract class BaseAnimation<T> : MonoBehaviour
    {
        [SerializeField] protected T reference;
        [SerializeField] private Sprite[] sprites;
        [SerializeField] private bool loop;
        [SerializeField] private int frameRate = 24;
        
        public event Action OnCompleted;

        private bool isPlaying;
        private float elapsedTime;
        private float frameRateDeltaTime;
        private int currentFrame;

        private void Awake()
        {
            frameRateDeltaTime = 1f / frameRate;
            Enable = false;
            isPlaying = false;
        }

        public float Play()
        {
            isPlaying = true;
            currentFrame = 0;
            elapsedTime = 0f;
            Enable = true;
            SetFrame(currentFrame);
            return 0.25f;
        }

        public void Stop()
        {
            isPlaying = false;
            Enable = false;
            OnCompleted?.Invoke();
        }

        private void Update()
        {
            if (!isPlaying || sprites.Length == 0)
                return;

            elapsedTime += Time.deltaTime;

            while (elapsedTime >= frameRateDeltaTime)
            {
                elapsedTime -= frameRateDeltaTime;
                currentFrame++;

                if (currentFrame >= sprites.Length)
                {
                    if (loop)
                    {
                        currentFrame = 0;
                    }
                    else
                    {
                        currentFrame = sprites.Length - 1;
                        SetFrame(currentFrame);
                        Stop();
                        return;
                    }
                }

                SetFrame(currentFrame);
            }
        }

        private void SetFrame(int frame)
        {
            Sprite sprite = sprites[frame];
            OnUpdateFrame(sprite);
        }
        
        protected abstract void OnUpdateFrame(Sprite sprite);
        
        protected abstract bool Enable { set; }
    }
}
using System;
using UnityEngine;
using UnityEngine.UI;

namespace _KITSystem.Schedule
{
    public class UIAnimation : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private Sprite[] sprites;
        [SerializeField] private bool loop;
        [SerializeField] private int frameRate = 24;
        
        public event Action OnCompleted;

        private bool isPlaying;
        private float elapsedTime;
        private float frameRateDeltaTime;
        private int currentFrame;

        public bool IsPlaying => isPlaying;

        private void Awake()
        {
            frameRateDeltaTime = 1f / frameRate;
           
            Stop();
        }

        public void Play()
        {
            isPlaying = true;
            currentFrame = 0;
            elapsedTime = 0f;
            image.enabled = true;
            SetFrame(currentFrame);
        }

        public void Stop()
        {
            isPlaying = false;
            image.enabled = false;
         
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
            image.sprite = sprite;
        }
    }
}
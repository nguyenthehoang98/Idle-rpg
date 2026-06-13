using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Games.GamePlay.AnimationSystem
{
    public sealed class UnitAnimation
    {
        public event Action<State, Direction> OnAnimationStart; 
        public event Action<State, Direction> OnAnimationTrigger; 
        public event Action<State, Direction> OnAnimationEnd; 
        
        private SpriteRenderer renderer;
        private Dictionary<int, AnimationClipData> clips = new Dictionary<int, AnimationClipData>();
        private AnimationClipData currentClipData;

        public bool IsPaused { get; set; } = true;
        public float TimeScale { get; set; } = 1f;

        private float elapsedTime;
        private int frameIndex;
        
        private State state;
        private Direction direction;

        public UnitAnimation(SpriteRenderer renderer)
        {
            this.renderer = renderer;
        }

        public void Import(AnimationAsset asset, State state)
        {
            if (asset == null) return;

            foreach (var clipData in asset.Clips)
            {
                clips[GetKey(state, clipData.direction)] = clipData;
            }
        }
        
        public void Tick(float deltaTime)
        {
            if (currentClipData == null)
                return;

            if (IsPaused) return;

            elapsedTime += deltaTime * TimeScale;

            float frameTime = 1f / currentClipData.fps;

            if (elapsedTime < frameTime)
                return;

            elapsedTime -= frameTime;

            frameIndex++;

            if (frameIndex >= currentClipData.frames.Length)
            {
                if (currentClipData.loop)
                {
                    frameIndex = 0;
                }
                else
                {
                    OnAnimationEnd?.Invoke(state, direction);
                    return;
                }
            }

            renderer.sprite = currentClipData.frames[frameIndex];

            if (frameIndex == currentClipData.frameEvent)
            {
                OnAnimationTrigger?.Invoke(state, direction);
            }
        }

        public bool PlayAnimation(State animationState)
        {
            if (IsPaused) return false;
            
            return PlayAnimation(animationState, direction);
        }

        public bool PlayAnimation(State animationState, Direction animationDirection)
        {
            if (IsPaused) return false;

            if (animationState == state && animationDirection == direction)
                return false;

            if (!clips.TryGetValue(GetKey(animationState, animationDirection), out var clip))
                return false;
            
            state = animationState;
            direction = animationDirection;
            currentClipData = clip;
            
            elapsedTime = 0;
            frameIndex = 0;

            if (currentClipData.frames.Length > 0)
            {
                renderer.sprite = currentClipData.frames[0];
                renderer.transform.localScale = new Vector3(currentClipData.flip ? -1 : 1, 1, 1);
            }
            
            OnAnimationStart?.Invoke(state, direction);

            return true;
        }

        private static int GetKey(State state, Direction dir)
        {
            return ((int)state << 8) | (int)dir;
        }
    }
}
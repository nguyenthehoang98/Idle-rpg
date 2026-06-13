using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Games.AnimationSystem
{
    public class UnitAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private AnimationAsset idleAnimationAsset;
        [SerializeField] private AnimationAsset walkAnimationAsset;
        [SerializeField] private AnimationAsset attackAnimationAsset;

        public event Action<State> OnAnimationTrigger; 
        public event Action<State> OnAnimationEnd;
        public event Action<(Texture normal, Texture hdr)> OnAnimationStart;
       
        public bool IsPaused { get; set; }
        public float TimeScale { get; set; } = 1f;

        private Dictionary<int, AnimationClipData> clipMap;
        private AnimationClipData currentClipData;

        private float timer;
        private int frameIndex;

        private State currentState;
        private Direction currentDirection;
        private void Awake()
        {
            clipMap = new Dictionary<int, AnimationClipData>();

            void Load(AnimationAsset asset, State state)
            {
                if (asset == null) return;

                foreach (var clip in asset.Clips)
                {
                    int key = GetKey(state, clip.direction);
                    clipMap[key] = clip;
                }
            }

            Load(idleAnimationAsset, State.Idle);
            Load(walkAnimationAsset, State.Walk);
            Load(attackAnimationAsset, State.Attack);
        }

        // Sau cho cùng update từ 1 tickable để đảm bảo cùng update frame
        private void Update()
        {
            if (currentClipData == null)
                return;

            if (IsPaused) return;

            timer += Time.deltaTime * TimeScale;

            float frameTime = 1f / currentClipData.fps;

            if (timer < frameTime)
                return;

            timer -= frameTime;

            frameIndex++;

            if (frameIndex >= currentClipData.frames.Length)
            {
                if (currentClipData.loop)
                {
                    frameIndex = 0;
                }
                else
                {
                    OnOneShotFinished();
                    return;
                }
            }

            spriteRenderer.sprite = currentClipData.frames[frameIndex];

            if (frameIndex == currentClipData.frameEvent)
            {
                OnAnimationTrigger?.Invoke(currentState);
            }
        }

        private void OnOneShotFinished()
        {
            OnAnimationEnd?.Invoke(currentState);
        }

        public int Play(State state)
        {
            if (IsPaused) return -1;
            
            return Play(state, currentDirection);
        }
        
        public int Play(State state, Vector3 position, Vector3 destination)
        {
            if (IsPaused) return -1;
            
            Direction direction = DirectionExtensions.GetDirection(position, destination);;
            if(state == State.Attack)
            {
                Debug.DrawLine(position, destination, Color.red, 2);
            }

            return Play(state, direction);
        }

        public int Play(State state, Direction dir)
        {
            if (IsPaused) return -1;
            
            int key = GetKey(state, dir);

            if (!clipMap.TryGetValue(key, out var clip))
                return -2;

            if (state == currentState && dir == currentDirection)
                return -4;

            currentState = state;
            currentDirection = dir;

            currentClipData = clip;

            timer = 0;
            frameIndex = 0;

            if (currentClipData.frames.Length > 0)
            {
                spriteRenderer.sprite = currentClipData.frames[0];
                spriteRenderer.transform.localScale =
                    new Vector3(currentClipData.flip ? -1 : 1, 1, 1);
            }

            OnAnimationStart?.Invoke((currentClipData.frames[0].texture, currentClipData.textureHDR));
            
            return 1;
        }

        private int GetKey(State state, Direction dir)
        {
            return ((int)state << 8) | (int)dir;
        }
    }
}
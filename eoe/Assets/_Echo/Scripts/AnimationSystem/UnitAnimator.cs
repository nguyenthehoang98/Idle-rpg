using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Echo.Scripts.AnimationSystem
{
    public class UnitAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private MonsterAnimationAsset idleAnimationAsset;
        [SerializeField] private MonsterAnimationAsset walkAnimationAsset;
        [SerializeField] private MonsterAnimationAsset attackAnimationAsset;

        public event Action<AnimState> OnAnimationTrigger; 
        public event Action<AnimState> OnAnimationEnd;
        public event Action<(Texture normal, Texture hdr)> OnAnimationStart;
       
        public bool IsPaused { get; set; }
        public float TimeScale { get; set; } = 1f;

        private Dictionary<int, SpriteAnimClip> clipMap;
        private SpriteAnimClip currentClip;

        private float timer;
        private int frameIndex;

        private AnimState currentState;
        private Direction currentDirection;
        private void Awake()
        {
            clipMap = new Dictionary<int, SpriteAnimClip>();

            void Load(MonsterAnimationAsset asset, AnimState state)
            {
                if (asset == null) return;

                foreach (var clip in asset.Clips)
                {
                    int key = GetKey(state, clip.direction);
                    clipMap[key] = clip;
                }
            }

            Load(idleAnimationAsset, AnimState.Idle);
            Load(walkAnimationAsset, AnimState.Walk);
            Load(attackAnimationAsset, AnimState.Attack);
        }

        // Sau cho cùng update từ 1 tickable để đảm bảo cùng update frame
        private void Update()
        {
            if (currentClip == null)
                return;

            if (IsPaused) return;

            timer += Time.deltaTime * TimeScale;

            float frameTime = 1f / currentClip.fps;

            if (timer < frameTime)
                return;

            timer -= frameTime;

            frameIndex++;

            if (frameIndex >= currentClip.frames.Length)
            {
                if (currentClip.loop)
                {
                    frameIndex = 0;
                }
                else
                {
                    OnOneShotFinished();
                    return;
                }
            }

            spriteRenderer.sprite = currentClip.frames[frameIndex];

            if (frameIndex == currentClip.frameEvent)
            {
                OnAnimationTrigger?.Invoke(currentState);
            }
        }

        private void OnOneShotFinished()
        {
            OnAnimationEnd?.Invoke(currentState);
        }

        public int Play(AnimState state)
        {
            if (IsPaused) return -1;
            
            return Play(state, currentDirection);
        }
        
        public int Play(AnimState state, Vector3 position, Vector3 destination)
        {
            if (IsPaused) return -1;
            
            Direction direction = DirectionExtensions.GetDirection(position, destination);;
            if(state == AnimState.Attack)
            {
                Debug.DrawLine(position, destination, Color.red, 2);
            }

            return Play(state, direction);
        }

        public int Play(AnimState state, Direction dir)
        {
            if (IsPaused) return -1;
            
            int key = GetKey(state, dir);

            if (!clipMap.TryGetValue(key, out var clip))
                return -2;

            if (state == currentState && dir == currentDirection)
                return -4;

            currentState = state;
            currentDirection = dir;

            currentClip = clip;

            timer = 0;
            frameIndex = 0;

            if (currentClip.frames.Length > 0)
            {
                spriteRenderer.sprite = currentClip.frames[0];
                spriteRenderer.transform.localScale =
                    new Vector3(currentClip.flip ? -1 : 1, 1, 1);
            }

            OnAnimationStart?.Invoke((currentClip.frames[0].texture, currentClip.textureHDR));
            
            return 1;
        }

        private int GetKey(AnimState state, Direction dir)
        {
            return ((int)state << 8) | (int)dir;
        }
    }
}
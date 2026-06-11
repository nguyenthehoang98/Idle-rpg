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

        public event Action<AnimState> OnAnimationEnd;
        public event Action<(Texture defaultTexture, Texture hdrTexture)> OnAnimationStart;
       
        public bool IsPaused { get; set; }
        public float TimeScale { get; set; } = 1f;

        private Dictionary<int, SpriteAnimClip> clipMap;
        private SpriteAnimClip currentClip;

        private float timer;
        private int frameIndex;
        private bool isPlayingOneShot;

        private AnimState currentState;
        private Direction8 currentDirection;

        private AnimState cachedState;
        private Direction8 cachedDirection;

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
        }

        private void OnOneShotFinished()
        {
            isPlayingOneShot = false;
            
            OnAnimationEnd?.Invoke(currentState);

            if (currentState == AnimState.Die) return;

            Play(cachedState, cachedDirection);
        }

        public int Play(AnimState state, Vector3 position, Vector3 destination)
        {
            return Play(state, GetDirection(position, destination));
        }

        public int Play(AnimState state, Direction8 dir)
        {
            int key = GetKey(state, dir);

            if (!clipMap.TryGetValue(key, out var clip))
                return -2;

            // Nếu đang play OneShot
            if (isPlayingOneShot)
            {
                // Chỉ update animation nền để sau này quay lại
                if (clip.loop)
                {
                    cachedState = state;
                    cachedDirection = dir;
                }

                return -3;
            }

            if (state == currentState &&
                dir == currentDirection)
                return -1;

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

            // Nếu là OneShot
            if (!currentClip.loop)
            {
                isPlayingOneShot = true;
            }
            else
            {
                cachedState = state;
                cachedDirection = dir;
            }

            OnAnimationStart?.Invoke((currentClip.frames[0].texture, currentClip.textureHDR));
            
            return 0;
        }

        private int GetKey(AnimState state, Direction8 dir)
        {
            return ((int)state << 8) | (int)dir;
        }
        
        static Direction8 GetDirection(Vector3 position, Vector3 destination)
        {
            Vector3 delta = destination - position;

            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

            if (angle >= -22.5f && angle < 22.5f) return Direction8.R;
            if (angle >= 22.5f && angle < 67.5f) return Direction8.TR;
            if (angle >= 67.5f && angle < 112.5f) return Direction8.T;
            if (angle >= 112.5f && angle < 157.5f) return Direction8.TL;
            if (angle >= 157.5f || angle < -157.5f) return Direction8.L;
            if (angle >= -157.5f && angle < -112.5f) return Direction8.BL;
            if (angle >= -112.5f && angle < -67.5f) return Direction8.B;
            return Direction8.BR;
        }
    }
}
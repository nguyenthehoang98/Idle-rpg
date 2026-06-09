using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Echo.Scripts.AnimationSystem
{
    public class SpriteAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private MonsterAnimationAsset idleAnimationAsset;
        [SerializeField] private MonsterAnimationAsset walkAnimationAsset;
        [SerializeField] private MonsterAnimationAsset attackAnimationAsset;

        public event Action<AnimState> OnOneShotAnimationEnd;
        
        private Dictionary<int, SpriteAnimClip> clipMap;
        private SpriteAnimClip currentClip;

        private float timer;
        private int frameIndex;
        private float scaleTime = 1f;
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

        private void Start()
        {
            int result = Play(AnimState.Walk, Direction8.T);
        }

        private void Update()
        {
            if (currentClip == null)
                return;

            timer += Time.deltaTime * scaleTime;

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

            spriteRenderer.sprite =
                currentClip.frames[frameIndex];
        }

        private void OnOneShotFinished()
        {
            isPlayingOneShot = false;
            
            OnOneShotAnimationEnd?.Invoke(currentState);

            if (currentState == AnimState.Die) return;

            Play(cachedState, cachedDirection);
        }

        public void SetTimeScale(float timeScale) => scaleTime = timeScale;

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

            return 0;
        }

        private int GetKey(AnimState state, Direction8 dir)
        {
            return ((int)state << 8) | (int)dir;
        }
    }
}
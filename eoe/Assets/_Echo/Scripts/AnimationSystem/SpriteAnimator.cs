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
        [SerializeField] private MonsterAnimationAsset attack1AnimationAsset;

        private Dictionary<int, SpriteAnimClip> clipMap;
        private SpriteAnimClip currentClip;

        private float timer;
        private int frameIndex;

        private float scaleTime = 1f;

        private AnimState currentState = AnimState.Die;
        private Direction8 currentDirection;

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
            Load(attack1AnimationAsset, AnimState.Attack1);
        }

        private void Start()
        {
            int result = Play(AnimState.Walk, Direction8.T);
            Debug.Log(result);
        }

        private void Update()
        {
            if (currentClip == null || currentClip.frames == null)
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
                    frameIndex = currentClip.frames.Length - 1;
                }
            }

            spriteRenderer.sprite = currentClip.frames[frameIndex];
        }
        
        public void SetTimeScale(float timeScale) => scaleTime = timeScale;

        public int Play(AnimState state, Direction8 dir)
        {
            if (state == currentState && dir == currentDirection)
                return -1;

            currentState = state;
            currentDirection = dir;

            int key = GetKey(state, dir);

            if (!clipMap.TryGetValue(key, out currentClip))
                return -2;

            timer = 0;
            frameIndex = 0;

            if (currentClip.frames.Length > 0)
            {
                spriteRenderer.sprite = currentClip.frames[0];
                spriteRenderer.transform.localScale = new Vector3(currentClip.flip ? -1 : 1, 1, 1);
            }

            return 0;
        }

        private int GetKey(AnimState state, Direction8 dir)
        {
            return ((int)state << 8) | (int)dir;
        }
    }
}
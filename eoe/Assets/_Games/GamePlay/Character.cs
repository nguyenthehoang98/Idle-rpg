using System;
using System.Collections;
using System.Collections.Generic;
using _Games.GamePlay.AnimationSystem;
using _Games.GamePlay.SkillSystem;
using _KITSystem.Utils;
using UnityEngine;

namespace _Games.GamePlay
{
    public class Character : MonoBehaviour
    {
        [Header("Renderer")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private HDRRenderer hdrRenderer;
        [Header("Skill")]
        [SerializeField] private SkillAsset skillAsset;
        [Header("Animation")]
        [SerializeField] private AnimationAsset idleAnimationAsset;
        [SerializeField] private AnimationAsset attackAnimationAsset;

        private UnitAnimation unitAnimation;
        private readonly Queue<Action> actionQueue = new Queue<Action>();

        private void Awake()
        {
            unitAnimation = new UnitAnimation(spriteRenderer);
            unitAnimation.Import(idleAnimationAsset, State.Idle);
            unitAnimation.Import(attackAnimationAsset, State.Attack);
            unitAnimation.OnAnimationStart += (state, direction) =>
            {
                AnimationClipData[] clips =
                    state == State.Attack ? attackAnimationAsset.Clips : idleAnimationAsset.Clips;

                for (int i = 0; i < clips.Length; i++)
                {
                    AnimationClipData clipData = clips[i];

                    if (clipData.direction == direction)
                    {
                        hdrRenderer.SetTexture(clipData.frames[0].texture, clipData.textureHDR);
                    }
                }
            };
            unitAnimation.OnAnimationTrigger += (state, direction) =>
            {
                if (state == State.Attack && actionQueue.Count > 0) actionQueue.Dequeue().Invoke();
            };
            unitAnimation.OnAnimationEnd += (state, direction) =>
            {
                bool shouldIdle = true;
                if (state == State.Attack && shouldIdle) unitAnimation.PlayAnimation(State.Idle);
            };
        }

        private void Start()
        {
            StartCoroutine(AutoCast());
        }

        public void Initialize()
        {
            AnimationTickable.Add(unitAnimation);
        }

        public void Destroy()
        {
            AnimationTickable.Remove(unitAnimation);
            
            unitAnimation = null;
        }

        private IEnumerator AutoCast()
        {
            while (true)
            {
                yield return new WaitForSeconds(2);
                
                Vector3 position = transform.position;
                Vector3 destination = new Vector3(RandomUtils.Range(-1f, 1f), RandomUtils.Range(-1f, 1f)).normalized * 5;

                int result = unitAnimation.PlayAnimation(State.Attack, DirectionExtensions.GetDirection(position, destination));
                
                if (result > 0)
                {
                    Action action = () =>
                    {
                        Debug.Log("CastSkill");
                        SkillTickable.CastSkill(skillAsset.SkillData, position, destination);
                    };
                    actionQueue.Enqueue(action);
                }
            }
        }

        public void Activate(float duration)
        {
            unitAnimation.IsPaused = false;
            hdrRenderer.Activate(duration);
        }

        public void Deactivate(float duration)
        {
            unitAnimation.IsPaused = true;
            hdrRenderer.Deactivate(duration);
        }
    }
}
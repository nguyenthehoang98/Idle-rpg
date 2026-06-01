using System;
using System.Collections;
using _KITSystem.Resource;
using _KITSystem.Utils;
using Animancer;
using MoreMountains.Feedbacks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _FightCode.Battle.View
{
    public class MonsterAnimation : MonoBehaviour
    {
        private static readonly int HitEffectBlend = Shader.PropertyToID("_HitEffectBlend");

        // move at transform
        [SerializeField] private Transform flip; // flip
        [SerializeField] private new SpriteRenderer renderer;
        [SerializeField] private AnimancerComponent animancer;
        [SerializeField] private AnimationClip moveAnimationClip;
        [SerializeField] private AnimationClip attackAnimationClip;
        [SerializeField] private AnimationClip beHitAnimationClip;
        [SerializeField] private AnimationClip deathAnimationClip;

        private AnimancerState state;
        private Action onDeathCallback;

        private Vector3 localScale;
        private bool defaultFace = true; // false: left, true: right

        private MaterialPropertyBlock hitEffectProperty;
        private Coroutine hitCoroutine;
        private MMF_Player currentBeHit;

        private void Awake()
        {
            hitEffectProperty = new MaterialPropertyBlock();
            
            animancer = GetComponent<AnimancerComponent>();
            localScale = flip.localScale;
        }

        private void OnValidate()
        {
            // animancer = GetComponentInChildren<AnimancerComponent>();
            /*
            if (animancer.Animator == null)
                animancer.Animator = GetComponent<Animator>();*/
            /*Transform root = flip.Find("Root");
            root.localPosition = new Vector3(0, -0.8f, 0);*/

            /*renderer = GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.name = "Renderer";
            }

            moveAnimationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/_FightSource/Battle/Monster/Animation/Move.anim");
            attackAnimationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/_FightSource/Battle/Monster/Animation/Attack Melee.anim");
            beHitAnimationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/_FightSource/Battle/Monster/Animation/BeHit.anim");
            deathAnimationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/_FightSource/Battle/Monster/Animation/Death.anim");*/
            
            /*Transform root = transform.Find("Flip/Root");
            Animator animator = root.gameObject.AddComponent<Animator>();
            
            animancer = root.gameObject.AddComponent<AnimancerComponent>();
            animancer.Animator = animator;*/
            
            /*AnimancerComponent[] components = flip.GetComponentsInChildren<AnimancerComponent>(true);
            if (components.Length > 1)
            {
                for (int i = 1; i < components.Length; i++)
                {
                    Object.DestroyImmediate(components[i]);
                }
            }*/
        }

        /*[MenuItem("Tools/A")]
        public static void Rm()
        {
            foreach (Object obj in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(obj);

                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);

                var components = prefabRoot.GetComponentsInChildren<AnimancerComponent>(true);

                Debug.Log($"[{components.Length}] {path}");

                for (int i = components.Length - 1; i >= 1; i--)
                {
                    Object.DestroyImmediate(components[i]);
                }

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }*/

        public void SetPosition(Vector3 pos)
        {
            transform.position = pos;

            bool right = pos.x < 0;
            if (right != defaultFace)
            {
                defaultFace = right;
                flip.localScale = new Vector3(localScale.x * (right ? 1 : -1), localScale.y, localScale.z);
            }
        }

        public void BeHit()
        {
            if (hitCoroutine != null) StopCoroutine(hitCoroutine);

            renderer.GetPropertyBlock(hitEffectProperty);

            hitCoroutine = this.LerpNormalize(0, 1, 0.1f, f =>
            {
                hitEffectProperty.SetFloat(HitEffectBlend, f);

                renderer.SetPropertyBlock(hitEffectProperty);
            }, () =>
            {
                hitEffectProperty.SetFloat(HitEffectBlend, 0);

                renderer.SetPropertyBlock(hitEffectProperty);
            });
            
            currentBeHit = BattleEffect.Instance.SpawnHitEffect(transform.position);
        }

        public void Dead(Vector3 force, Action onDestroy)
        {
            onDeathCallback = onDestroy;
            
            if (state != null) state.Stop();
            
            state = animancer.Play(deathAnimationClip);
            
            StartCoroutine(Knockback(force, state.Duration));

            if (currentBeHit != null && currentBeHit.IsPlaying)
            {
                currentBeHit.StopFeedbacks();
                
                KitPool.Destroy(currentBeHit.gameObject);
            }
        }

        public void OnDeathEvent()
        {
            gameObject.SetActive(false);
            
            onDeathCallback?.Invoke();
        }

        private IEnumerator Knockback(Vector3 force, float duration)
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + force;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);

                // EaseOutBack
                float ease = EaseOutBack(t);

                transform.position = Vector3.LerpUnclamped(
                    startPos,
                    endPos,
                    ease);

                yield return null;
            }

            transform.position = endPos;
        }
        
        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;

            return 1f + c3 * Mathf.Pow(t - 1f, 3)
                      + c1 * Mathf.Pow(t - 1f, 2);
        }

        public void Idle()
        {
            // code
            if (state != null) state.Stop();
        }

        public void Move()
        {
            if (state != null) state.Stop();
            state = animancer.Play(moveAnimationClip);
        }
    }
}

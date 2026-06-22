using System;
using System.Collections;
using System.Collections.Generic;
using _Game.Configs;
using _Game.GamePlay.SkillSystem;
using _Game.GamePlay.SoundSystem;
using _KITSystem.Resource;
using _KITSystem.SkillSystem.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Game.GamePlay
{
    public class Weapon : MonoBehaviour
    {
        private static readonly int Attack = Animator.StringToHash("Attack");

        [SerializeField] private Transform muzzle;
        [SerializeField] private Animator animator;
        [SerializeField] private float animationClipDuration;
        [SerializeField] private float weaponRotationDuration = 0.15f;
        [SerializeField] private AnimationCurve weaponRotationCurve;

        private float TimeScale { get; set; } = 1;
        private float DeltaTime { get; set; } = 0.0334f;
        public bool IsActivated { private get; set; } = false;
        private SkillData SkillData { get; set; }
        private WeaponData WeaponData { get; set; }

        private Vector3 destination;
        private bool attacking;
        
        private HashSet<GameObject> objects = new HashSet<GameObject>();
        private HashSet<string> objectsName = new HashSet<string>();

        private void Awake()
        {
            DeltaTime = Time.deltaTime;
        }

        private void Start()
        {
            animator.enabled = false;
        }

        private void OnDestroy()
        {
            foreach (var assetName in objectsName)
            {
                AssetBundleManager.UnCache(assetName);
            }

            objectsName = null;

            foreach (var go in objects)
            {
                Pool.UnRegisterPool(go);
            }

            objects = null;
        }

        public async UniTask Initialize(WeaponData weaponData, float timeScale, float deltaTime)
        {
            WeaponData = weaponData;
            SkillData = weaponData.skillData;
            TimeScale = timeScale;
            DeltaTime = deltaTime;

            if (!string.IsNullOrEmpty(WeaponData.prefabName))
            {
                GameObject go = await AssetBundleManager.GetAssetCached<GameObject>(weaponData.prefabName);
                if (go != null)
                {
                    objects.Add(go);
                    objectsName.Add(WeaponData.prefabName);
                }
            }

            if (!string.IsNullOrEmpty(WeaponData.attackAudioClip))
            {
                AudioClip audioClip = await AssetBundleManager.GetAssetCached<AudioClip>(WeaponData.attackAudioClip);
                if (audioClip != null)
                {
                    objectsName.Add(WeaponData.attackAudioClip);
                }
            }

            if (!string.IsNullOrEmpty(WeaponData.projectileName))
            {
                GameObject go = await AssetBundleManager.GetAssetCached<GameObject>(WeaponData.projectileName);
                if (go != null)
                {
                    objects.Add(go);
                    objectsName.Add(WeaponData.projectileName);
                }
            }

            StartCoroutine(AutoAttack());
        }

        private IEnumerator AutoAttack()
        {
            while (true)
            {
                yield return new WaitForSeconds(WeaponData.cooldown / TimeScale);

                if (attacking || !IsActivated) continue;
                
                FindTargetType type = FindTargetType.Filter;
                if (SkillData.findTarget.type == FindTargetData.FilterType.Farthest)
                    type = FindTargetType.Farthest;
                else if (SkillData.findTarget.type == FindTargetData.FilterType.Nearest)
                    type = FindTargetType.Nearest;

                Vector3 position = MuzzlePosition();
                Vector3 center = Vector3.zero;
                
                SkillTickable.FindTarget(type, center, position,
                    SkillData.findTarget.radius, FilterEntity, out var result);

                int entity = -1;
                destination = Vector3.zero;
                if (result.Primary.IsValid)
                {
                    entity = result.Primary.Entity;
                    destination = result.Primary.Position;
                }
                else if (result.Secondary.IsValid)
                {
                    entity = result.Secondary.Entity;
                    destination = result.Secondary.Position;
                }

                if (entity == -1)
                {
#if UNITY_EDITOR
                    GizmosLine.Circle(center, SkillData.findTarget.radius, Color.red, WeaponData.cooldown / TimeScale, 36);
#endif
                    continue;
                }
                else
                {
#if UNITY_EDITOR
                    GizmosLine.Circle(center, SkillData.findTarget.radius, Color.green, WeaponData.cooldown / TimeScale, 36);
#endif
                }

                Vector3 direction = destination - position;
#if UNITY_EDITOR
                Debug.DrawRay(position, direction.normalized * SkillData.findTarget.radius, Color.magenta, 1);
#endif

                float angleFrom = transform.eulerAngles.z;
                float angleTo = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                float elapsedTime = 0;

                float angleDelta = Mathf.Abs(Mathf.DeltaAngle(angleFrom, angleTo));

                float dynamicDuration = Mathf.Lerp(
                    0f,
                    weaponRotationDuration,
                    angleDelta / 180f
                );

                while (elapsedTime <= dynamicDuration && IsActivated)
                {
                    float dt = DeltaTime / TimeScale;

                    elapsedTime += dt;
                    float t = Mathf.Clamp01(elapsedTime / dynamicDuration);
                    float s = weaponRotationCurve.Evaluate(t);

                    float angle = Mathf.LerpAngle(angleFrom, angleTo, s);
                    transform.eulerAngles = new Vector3(0, 0, angle);

                    yield return new WaitForSeconds(dt);
                }

                if (!IsActivated) yield break;

                OnPlay();

                transform.eulerAngles = new Vector3(0, 0, angleTo);

                animator.enabled = true;
                
                animator.Play(Attack, 0, 0);
                
                animator.speed = TimeScale * WeaponData.attackSpeed;
                
                attacking = true;
                
                yield return new WaitForSeconds(animationClipDuration / animator.speed);

                animator.enabled = false;
                
                attacking = false;
            }
        }

        private bool FilterEntity(int entity)
        {
            return true;
        }

        public async void ExecutePrivate()
        {
            if (attacking && IsActivated)
            {
                AudioClip clip = null;
                
                if (!string.IsNullOrEmpty(WeaponData.attackAudioClip))
                {
                    clip = await AssetBundleManager.GetAssetCached<AudioClip>(WeaponData.attackAudioClip);
                }
                SoundManager.Instance.PlayOneShot(clip, WeaponData.attackVolume);
                
                Vector3 position = MuzzlePosition();
                
                Vector3 target = GetDestination(position, destination);

                SkillStatData statData = new SkillStatData
                {
                    Attack = WeaponData.attack
                };
                
                SkillTickable.CastSkill(SkillData, statData, position, target);

                OnExecute();
            }
        }

        protected virtual void OnPlay()
        {
        }

        protected virtual void OnExecute()
        {
        }

        protected virtual Vector3 GetDestination(Vector3 from, Vector3 to)
        {
            return to;
        }
        
        protected virtual Vector3 MuzzlePosition()
        {
            return muzzle != null ? muzzle.position : Vector3.zero;
        }
    }
}
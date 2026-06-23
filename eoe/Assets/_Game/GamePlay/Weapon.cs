using System.Collections;
using System.Collections.Generic;
using _Game.Configs;
using _Game.GamePlay.Manager;
using _KITSystem.Resource;
using _KITSystem.SkillSystem.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

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

        public int WeaponId => weaponData.id;
        public int CurrentLevel { get; private set; }
        
        private IQuery query = new EntityQuery();
        private SkillData skillData;
        private WeaponData weaponData;
        private float timeScale = 1;
        private float deltaTime = 0.0334f;
        private bool isActivated;

        private Vector3 destination;
        private bool attacking;

        private HashSet<GameObject> objects = new HashSet<GameObject>();
        private HashSet<string> objectsName = new HashSet<string>();

        private WeaponUpgradeData current;
        private WeaponUpgradeData levelUp;
        private WeaponUpgradeData powerX2;
        private WeaponUpgradeData powerX3;

        private void Awake()
        {
            deltaTime = Time.deltaTime;
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

        public async UniTask Initialize(WeaponData weaponData, WeaponUpgradeData powerx2, WeaponUpgradeData powerx3,
            float timeScale, float deltaTime)
        {
            powerX2 = powerx2;
            powerX3 = powerx3;
            this.weaponData = weaponData;
            skillData = weaponData.skillData;
            this.timeScale = timeScale;
            this.deltaTime = deltaTime;
            current = new WeaponUpgradeData();
            if (!string.IsNullOrEmpty(this.weaponData.prefabName))
            {
                GameObject go = await AssetBundleManager.GetAssetCached<GameObject>(weaponData.prefabName);
                if (go != null)
                {
                    objects.Add(go);
                    objectsName.Add(this.weaponData.prefabName);
                }
            }

            if (!string.IsNullOrEmpty(this.weaponData.attackAudioClip))
            {
                AudioClip audioClip = await AssetBundleManager.GetAssetCached<AudioClip>(this.weaponData.attackAudioClip);
                if (audioClip != null)
                {
                    objectsName.Add(this.weaponData.attackAudioClip);
                }
            }

            if (!string.IsNullOrEmpty(this.weaponData.projectileName))
            {
                GameObject go = await AssetBundleManager.GetAssetCached<GameObject>(this.weaponData.projectileName);
                if (go != null)
                {
                    objects.Add(go);
                    objectsName.Add(this.weaponData.projectileName);
                }
            }

            StartCoroutine(AutoAttack());
        }

        public void SetLevel(int level)
        {
            CurrentLevel = level;
            isActivated = level > 0;

            RefreshUpgradeData();
        }

        public void Increase(WeaponUpgradeData upgradeData)
        {
            levelUp.Increase(upgradeData);

            RefreshUpgradeData();
        }

        private void RefreshUpgradeData()
        {
            current = levelUp;
            if (CurrentLevel == 2) current.Increase(powerX2);
            if (CurrentLevel == 3) current.Increase(powerX3);
        }

        private IEnumerator AutoAttack()
        {
            while (true)
            {
                yield return new WaitForSeconds(weaponData.cooldown * (1 - current.cooldownReduce) / timeScale);

                if (attacking || !isActivated) continue;

                FindTargetType type = FindTargetType.Filter;
                if (skillData.findTarget.type == FindTargetData.FilterType.Farthest)
                    type = FindTargetType.Farthest;
                else if (skillData.findTarget.type == FindTargetData.FilterType.Nearest)
                    type = FindTargetType.Nearest;

                Vector3 position = MuzzlePosition();
                Vector3 center = Vector3.zero;

                float radius = skillData.findTarget.radius;
                query.FindTarget(type, center, position, radius, FilterEntity, out var result);

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
                    GizmosLine.Circle(center, radius, Color.red, weaponData.cooldown / timeScale, 36);
#endif
                    continue;
                }
                else
                {
#if UNITY_EDITOR
                    GizmosLine.Circle(center, radius, Color.green, weaponData.cooldown / timeScale, 36);
#endif
                }

                Vector3 direction = destination - position;
#if UNITY_EDITOR
                Debug.DrawRay(position, direction.normalized * skillData.findTarget.radius, Color.magenta, 1);
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

                while (elapsedTime <= dynamicDuration && isActivated)
                {
                    float dt = deltaTime / timeScale;

                    elapsedTime += dt;
                    float t = Mathf.Clamp01(elapsedTime / dynamicDuration);
                    float s = weaponRotationCurve.Evaluate(t);

                    float angle = Mathf.LerpAngle(angleFrom, angleTo, s);
                    transform.eulerAngles = new Vector3(0, 0, angle);

                    yield return new WaitForSeconds(dt);
                }

                if (!isActivated) yield break;

                OnPlay();

                transform.eulerAngles = new Vector3(0, 0, angleTo);

                animator.enabled = true;

                animator.Play(Attack, 0, 0);

                float attackSpeed = weaponData.attackSpeed + current.attackSpeed;

                animator.speed = timeScale * attackSpeed;

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
            if (attacking && isActivated)
            {
                AudioClip clip = null;

                if (!string.IsNullOrEmpty(weaponData.attackAudioClip))
                {
                    clip = await AssetBundleManager.GetAssetCached<AudioClip>(weaponData.attackAudioClip);
                }

                SoundManager.Instance.PlayOneShot(clip, weaponData.attackVolume);

                Vector3 position = MuzzlePosition();

                Vector3 target = GetDestination(position, destination);

                SkillRuntimeData runtimeData = new SkillRuntimeData
                {
                    Attack = (1 + current.damagePercent) * weaponData.attack,
                    CritChance = current.critChance + weaponData.critChance,
                    CritDamage = current.critDamage + weaponData.critDamage,
                    ParallelCount = current.parallelCount,
                    SpreadCount = current.spreadCount,
                    SpreadDamagePercent = current.spreadDamagePercent,
                    PiercingCount = current.piercingCount,
                    ExplosiveRadius = current.explosiveRadius,
                    ExplosiveDamagePercent = current.explosiveDamagePercent,
                    BounceCount = current.bounceCount,
                    BounceDamagePercent = current.bounceDamagePercent,
                    KillInstantBelowHealthPercent = current.killInstantBelowHealthPercent,
                };

                SkillManager.CastSkill(skillData, runtimeData, position, target);

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
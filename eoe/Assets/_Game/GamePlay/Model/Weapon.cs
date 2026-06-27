using System.Collections;
using System.Collections.Generic;
using _Game.Configs;
using _Game.GamePlay.Data;
using _Game.GamePlay.Entity;
using _Game.GamePlay.Manager;
using _Game.GamePlay.Utils;
using _KITSystem.Entity;
using _KITSystem.Resource;
using _KITSystem.SkillSystem.Core;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.GamePlay.Model
{
    public class Weapon : MonoBehaviour
    {
        private static readonly int AttackAnimator = Animator.StringToHash("Attack");
        private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");

        [Header("Outline")]
        [SerializeField] private SpriteRenderer spOutline;
        [SerializeField] private Color color2 = new Color(1, 1, 0, 1);
        [SerializeField] private Color color3 = new Color(1, 0, 1, 1);
        [SerializeField] private float outlineColorDuration = 0.2f;
        [Header("Element")]
        [SerializeField] private Transform muzzle;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform rotatePivot;
        [SerializeField] private AnimationCurve rotationCurve;
        [SerializeField] private float rotationDuration;
        
        private HashSet<string> names = new HashSet<string>();

        private int entityTarget;
        private SkillData skillData;
        private WeaponUpgradeData current;
        private WeaponUpgradeData levelUp;
        private WeaponUpgradeData powerX2;
        private WeaponUpgradeData powerX3;

        private IQuery query;
        private Coroutine coroutine;
        private MaterialPropertyBlock propertyBlock;
        private AudioClip attackAudioClip;
        private float attackVolume;
        
        private float attackCooldown;
        private float attackSpeed;
        private float attack;
        private float critChance;
        private float critDamage;

        private Vector3 destination;
        private int level;
        private bool isAttacking;
        private bool isActivated;

        public float TimeScale { get; set; } = 1f;
        public float DeltaTime { get; set; } = 0.034f; // = delta / timescale
        public int WeaponLevel
        {
            set
            {
                bool shouldUpdateOutlineColor = level != value;
                
                level = value;
                
                isActivated = value > 0;
                
                RefreshUpgradeData(shouldUpdateOutlineColor);
            }
        }

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        private void OnDestroy()
        {
            foreach (var assetName in names)
            {
                AssetBundleManager.UnCache(assetName);
            }

            names = null;
        }

        public async UniTask Initialize(WeaponData weaponData, WeaponUpgradeData upgradeDataX2, WeaponUpgradeData upgradeDataX3, float faceFlip)
        {
            powerX2 = upgradeDataX2;
            powerX3 = upgradeDataX3;

            attack = weaponData.attack;
            critChance = weaponData.critChance;
            critDamage = weaponData.critDamage;
            attackCooldown = weaponData.cooldown;
            skillData = weaponData.skillData;
            attackSpeed = weaponData.attackSpeed;
            attackVolume = weaponData.attackVolume;
            query = new EntityQuery();
            
            GameObject go = null;
            
            rotatePivot.localScale = new Vector3(faceFlip, 1, 1);

            if (names.Add(skillData.prefabName))
            {
                go = await AssetBundleManager.GetAssetCached<GameObject>(skillData.prefabName);
                Pool.RegisterPool(go, true);
                Pool.Destroy(Pool.Instantiate(go));
            }

            if (names.Add(skillData.impactName))
            {
                go = await AssetBundleManager.GetAssetCached<GameObject>(skillData.impactName);
                Pool.RegisterPool(go, true);
                Pool.Destroy(Pool.Instantiate(go));
            }

            if (names.Add(weaponData.attackAudioClip))
            {
                attackAudioClip = await AssetBundleManager.GetAssetCached<AudioClip>(weaponData.attackAudioClip);
            }

            StartCoroutine(AutoAttack());
            
            RefreshUpgradeData(true);
        }

        public void IncreaseUpgradeData(WeaponUpgradeData upgradeData)
        {
            levelUp.Increase(upgradeData);
            
            RefreshUpgradeData(false);
        }
        
        public void ExecuteAnimation()
        {
            if (!isActivated || !isAttacking) return;

            SoundManager.Instance.PlayOneShot(attackAudioClip, attackVolume);

            Vector3 muzzlePosition = MuzzlePosition();
            Vector3 destinationPosition = Destination(muzzlePosition, destination);
            SkillRuntimeData runtimeData = new SkillRuntimeData
            {
                Attack = (1 + current.damagePercent) * attack,
                CritChance = current.critChance + critChance,
                CritDamage = current.critDamage + critDamage,
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
            
            SkillManager.CastSkill(skillData, runtimeData, muzzlePosition, destinationPosition, entityTarget);
        }

        public void EndAnimation()
        {
            isAttacking = false;
        }

        private void RefreshUpgradeData(bool updateOutline)
        {
            current = levelUp;
            Color color = Color.clear;

            if (level == 2)
            {
                current.Increase(powerX2);
                color = color2;
            }
            else if (level == 3)
            {
                current.Increase(powerX3);
                color = color3;
            }

            if (updateOutline)
            {
                if (coroutine != null) StopCoroutine(coroutine);
                coroutine = StartCoroutine(ChangeColor(color));
            }
        }

        private IEnumerator ChangeColor(Color targetColor)
        {
            float elapsedTime = 0f;

            spOutline.GetPropertyBlock(propertyBlock);
            Color color = propertyBlock.GetColor(OutlineColor);
            
            while (elapsedTime < outlineColorDuration)
            {
                elapsedTime += DeltaTime;

                float t = Mathf.Clamp01(elapsedTime / outlineColorDuration);

                propertyBlock.SetColor(OutlineColor, Color.Lerp(color, targetColor, t));

                spOutline.SetPropertyBlock(propertyBlock);
                
                yield return new WaitForSeconds(DeltaTime);
            }
        }

        private IEnumerator AutoAttack()
        {
            while (true)
            {
                float cooldown = attackCooldown * (1 - current.cooldownReduce);

                yield return new WaitForSeconds(cooldown / TimeScale);

                if (isAttacking || !isActivated)
                {
#if UNITY_EDITOR
                    //Debug.LogWarning($"Weapon {name} in active '{isActivated}', attack '{isAttacking}'");    
#endif
                    continue;
                }

                FindTargetType type = FindTargetType.Filter;
                if (skillData.findTarget.type == FindTargetData.FilterType.Farthest)
                    type = FindTargetType.Farthest;
                else if (skillData.findTarget.type == FindTargetData.FilterType.Nearest)
                    type = FindTargetType.Nearest;

                Vector3 position = MuzzlePosition();
                Vector3 center = Vector3.zero;
                float radius = skillData.findTarget.radius;
                float sqrRadius = radius * radius;
                query.FindTarget(type, center, position, radius, (entity, float2) =>
                {
                    HealthData healthData = ComponentManager<HealthData>.Get(entity);
                    if (healthData.FutureHealth <= 0)
                    {
                        return false;
                    }
                    
                    float d = math.lengthsq(float2);
                    return d <= sqrRadius;
                }, out QueryResult result);

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

#if UNITY_EDITOR
                float gizmosDeltaTime = cooldown / TimeScale;
#endif

                entityTarget = entity;
                
                if (entity == -1)
                {
#if UNITY_EDITOR
                    GizmosLine.Circle(center, radius, Color.red, gizmosDeltaTime, 36);
                    //Debug.LogWarning($"Weapon {name} not found target");
#endif
                    continue;
                }
                else
                {
#if UNITY_EDITOR
                    GizmosLine.Circle(center, radius, Color.green, gizmosDeltaTime, 36);
#endif
                }

                Vector3 direction = destination - position;
#if UNITY_EDITOR
                Debug.DrawRay(position, direction.normalized * radius, Color.magenta, 1);
#endif

                float angleFrom = rotatePivot.eulerAngles.z;
                float angleTo = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                float angleDelta = Mathf.Abs(Mathf.DeltaAngle(angleFrom, angleTo));
                float dynamicDuration = Mathf.Lerp(0f, rotationDuration, angleDelta / 180f);

                float elapsedTime = 0;
                while (elapsedTime <= dynamicDuration && !isActivated)
                {
                    elapsedTime += DeltaTime;
                    float t = Mathf.Clamp01(elapsedTime / dynamicDuration);
                    float s = rotationCurve.Evaluate(t);
                    float a = Mathf.LerpAngle(angleFrom, angleTo, s);
                    rotatePivot.eulerAngles = new Vector3(0, 0, a);

                    yield return new WaitForSeconds(DeltaTime);
                }

                if (!isActivated) yield break;

                OnAttack();

                rotatePivot.eulerAngles = new Vector3(0, 0, angleTo);
                rotatePivot.localScale = new Vector3(1, Mathf.Abs(angleTo) <= 90 ? 1 : -1f, 1);
                
                animator.Play(AttackAnimator, 0, 0);

                float speed = attackSpeed + current.attackSpeed;

                animator.speed = TimeScale * speed;

                isAttacking = true;
            }
        }

        protected virtual Vector3 MuzzlePosition() => muzzle.position;
        
        protected virtual Vector3 Destination(Vector3 from, Vector3 to) => destination;

        protected virtual void OnAttack()
        {
        }

        protected virtual void OnExecute()
        {
        }
    }
}

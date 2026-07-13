using System;
using System.Collections;
using System.Collections.Generic;
using _Game.Configs;
using _Game.GamePlay.Data;
using _Game.GamePlay.Entity;
using _Game.GamePlay.Manager;
using _Game.GamePlay.Utils;
using _Game.GamePlay.View;
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

        [SerializeField] private SpriteRenderer model;
        [SerializeField] private Transform muzzle;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform rotatePivot;
        [SerializeField] private AnimationCurve rotationCurve;
        [SerializeField] private float rotationDuration;
        [SerializeField] private float outlineColorDuration = 0.2f;
        
        private HashSet<string> names = new HashSet<string>();

        private int entityTarget;
        private int currentGroup; // {0:1-2-3-4} {1:4-5-6-7} {2:7-8-9-10}
        private int[] currentLevel = new int[2]; // line 0 & line 1. tương ứng với idx của group
        private SkillData skillData;
        private WeaponUpgradeData current;
        private WeaponUpgradeData levelUp;
        private WeaponUpgradeData powerX2;
        private WeaponUpgradeData powerX3;
        private WeaponUpgradeData[] groups;
        private bool[] isUpgraded;
        private Dictionary<int, List<WeaponUpgradeData>> upgradesData;

        private IQuery query;
        private Coroutine coroutineUpdateColor;
        private Coroutine coroutineAutoAttack;
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

        private bool isPaused;
        public float TimeScale { get; set; } = 1f;
        public float DeltaTime { get; set; } = 0.034f; // = delta / timescale
        public int WeaponLevel
        {
            get => level;
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

        public async UniTask Initialize(WeaponData weaponData, Dictionary<int, List<WeaponUpgradeData>> dict,
            WeaponUpgradeData upgradeDataX2, WeaponUpgradeData upgradeDataX3, 
            float faceFlip)
        {
            powerX2 = upgradeDataX2;
            powerX3 = upgradeDataX3;
            upgradesData = dict;

            attack = weaponData.attack;
            critChance = weaponData.critChance;
            critDamage = weaponData.critDamage;
            attackCooldown = weaponData.cooldown;
            skillData = weaponData.skillData;
            attackSpeed = weaponData.attackSpeed;
            attackVolume = weaponData.attackVolume;
            query = new EntityQuery();
            currentLevel = new int[2] { 1, 1 };

            UpdateUpgradeDataGroup();
            
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

            coroutineAutoAttack = StartCoroutine(AutoAttack());
            RefreshUpgradeData(true);
        }

        public void SetPause(bool pause)
        {
            isPaused = pause;
            if (pause)
            {
                StopCoroutine(coroutineAutoAttack);
                isAttacking = false;
            }
            else
            {
                coroutineAutoAttack = StartCoroutine(AutoAttack());
            }
        }

        public void IncreaseUpgradeData(WeaponUpgradeData upgradeData)
        {
            int lv = upgradeData.level;

            if (lv == 1 || lv == 4 || lv == 7 || lv == 10)
            {
                for (int i = 0; i < currentLevel.Length; i++)
                    currentLevel[i] = lv + 1;
            }
            else
            {
                int gr = upgradeData.group;
                currentLevel[gr - 1] = lv + 1;
            }
            
            if (lv == 4 || lv == 7 || lv == 10)
            {
                currentGroup++;
                UpdateUpgradeDataGroup();
            }

            int idx = Array.IndexOf(groups, upgradeData);
            isUpgraded[idx] = true;
            
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
                ParallelDamagePercent = current.parallelDamagePercent,
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

        private void UpdateUpgradeDataGroup()
        {
            int[] idx = new int[4];
            if (currentGroup == 0) idx = new int[4] { 1, 2, 3, 4 };
            else if (currentGroup == 1) idx = new int[4] { 4, 5, 6, 7 };
            else if (currentGroup == 2) idx = new int[4] { 7, 8, 9, 10 };
            else
            {
                isUpgraded = new bool[0];
                groups = new WeaponUpgradeData[0];
                return;
            }

            isUpgraded = new bool[6];
            groups = new WeaponUpgradeData[6];
            groups[0] = upgradesData[idx[0]][0];
            groups[1] = upgradesData[idx[1]][0];
            groups[2] = upgradesData[idx[1]][1];
            groups[3] = upgradesData[idx[2]][0];
            groups[4] = upgradesData[idx[2]][1];
            groups[5] = upgradesData[idx[3]][0];
        }

        public List<CardItemData> GetUpgradeDataAvailable()
        {
            List<CardItemData> list = new List<CardItemData>();
            if (groups.Length == 0) return list;
            
            List<WeaponUpgradeData> listUpgradeData;
            if (currentLevel[0] == currentLevel[1])
            {
                int idx = currentLevel[0];
                if (upgradesData.TryGetValue(idx, out listUpgradeData))
                {
                    for (int i = 0; i < listUpgradeData.Count; i++)
                    {
                        list.Add(new CardItemData
                        {
                            Satellites = groups,
                            Current = listUpgradeData[i],
                            IsUpgraded = isUpgraded,
                            currentGroup = currentGroup
                        });
                    }
                }
            }
            else
            {
                if (upgradesData.TryGetValue(currentLevel[0], out listUpgradeData))
                {
                    list.Add(new CardItemData
                    {
                        Satellites = groups,
                        Current = listUpgradeData[0],
                        IsUpgraded = isUpgraded,
                        currentGroup = currentGroup
                    });
                }
                if (upgradesData.TryGetValue(currentLevel[1], out listUpgradeData))
                {
                    if (listUpgradeData.Count > 1)
                    {
                        list.Add(new CardItemData
                        {
                            Satellites = groups,
                            Current = listUpgradeData[1],
                            IsUpgraded = isUpgraded,
                            currentGroup = currentGroup
                        });
                    }
                    else
                    {
                        list.Add(new CardItemData
                        {
                            Satellites = groups,
                            Current = listUpgradeData[0],
                            IsUpgraded = isUpgraded,
                            currentGroup = currentGroup
                        });
                    }
                }
            }
            
            return list;
        }

        private void RefreshUpgradeData(bool updateOutline)
        {
            current = levelUp;
            Color color = Color.clear;

            if (level == 2)
            {
                current.Increase(powerX2);
                color = Const.WEAPON_OUTLINE_X2;
            }
            else if (level == 3)
            {
                current.Increase(powerX3);
                color = Const.WEAPON_OUTLINE_X3;
            }

            if (updateOutline)
            {
                if (coroutineUpdateColor != null) StopCoroutine(coroutineUpdateColor);
                
                coroutineUpdateColor = StartCoroutine(ChangeColor(color));
            }
        }

        private IEnumerator ChangeColor(Color targetColor)
        {
            float elapsedTime = 0f;

            model.GetPropertyBlock(propertyBlock);
            Color color = propertyBlock.GetColor(OutlineColor);
            
            while (elapsedTime < outlineColorDuration)
            {
                elapsedTime += DeltaTime;

                float t = Mathf.Clamp01(elapsedTime / outlineColorDuration);

                propertyBlock.SetColor(OutlineColor, Color.Lerp(color, targetColor, t));

                model.SetPropertyBlock(propertyBlock);
                
                yield return new WaitForSeconds(DeltaTime);
            }
        }

        private IEnumerator AutoAttack()
        {
            while (!isPaused)
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
                if (skillData.filterType == SkillData.FilterType.Farthest)
                    type = FindTargetType.Farthest;
                else if (skillData.filterType == SkillData.FilterType.Nearest)
                    type = FindTargetType.Nearest;

                Vector3 position = MuzzlePosition();
                Vector3 center = Vector3.zero;
                float radius = skillData.filterRadius;
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

                if (!isActivated || isPaused) 
                    continue;
                
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

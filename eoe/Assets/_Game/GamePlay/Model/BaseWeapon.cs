using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using _Game.Configs;
using _Game.GamePlay.Data;
using _Game.GamePlay.Entity;
using _Game.GamePlay.Manager;
using _Game.GamePlay.Utils;
using _Game.GamePlay.View;
using _KITSystem.Entity;
using _KITSystem.Resource;
using _KITSystem.SkillSystem.Core;
using _KITSystem.SkillSystem.Imp;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace _Game.GamePlay.Model
{
    public abstract class BaseWeapon : MonoBehaviour
    {
        private static readonly int OutlineColorProperty = Shader.PropertyToID("_OutlineColor");

        [SerializeField] private new SpriteRenderer renderer;
        [SerializeField] private float adjustOutlineColorDuration = 0.2f;
        [SerializeField] protected TrajectoryData trajectory;
        [SerializeField] protected Transform rotatePivot;
        [SerializeField] private AnimationCurve rotationCurve;
        [SerializeField] private float rotationDuration = 0.15f;
        [SerializeField] private Transform muzzle;

        private WeaponUpgradeData current;
        private WeaponUpgradeData levelUp;
        private WeaponUpgradeData powerX2;
        private WeaponUpgradeData powerX3;

        private Dictionary<int, List<WeaponUpgradeData>> upgradesData;
        private WeaponUpgradeData[] groups;
        private bool[] isUpgraded;
        private int[] currentLevel = new int[2];

        private IQuery query;
        private Coroutine adjustOutlineColorCoroutine;
        private MaterialPropertyBlock propertyBlock;
        
        private AudioClip audioClip;
        private bool isCachedAudioClip;
        
        private Coroutine attackCoroutine;
        private Vector3 destination;
        private int entity;

        private int currentGroup; // {0:1-2-3-4} {1:4-5-6-7} {2:7-8-9-10}
        private int level;

        protected WeaponUpgradeData CurrentUpgradeData => current;
        protected SkillData SkillData { get; private set; }
        protected WeaponData WeaponData { get; private set; }
        protected float DeltaTime { get; private set; } = 0.034f;
        protected float TimeScale { get; private set; } = 1f;
        protected bool IsPaused { get; private set; }
        protected bool IsActivated { get; private set; }
        protected bool IsAttacking { get; private set; }

        public int Level
        {
            get => level;
            set
            {
                bool shouldUpdateOutlineColor = level != value;
                
                level = value;
                
                IsActivated = value > 0;
                
                UpgradeData(shouldUpdateOutlineColor);
            }
        }

        public virtual async UniTask Initialize(WeaponData weaponData, Dictionary<int, List<WeaponUpgradeData>> dict,
            WeaponUpgradeData upgradeDataX2, WeaponUpgradeData upgradeDataX3,
            float faceFlip)
        {
            powerX2 = upgradeDataX2;
            powerX3 = upgradeDataX3;
            upgradesData = dict;
            WeaponData = weaponData;
            SkillData = weaponData.skillData;
            query = new EntityQuery();
            currentLevel = new int[2] { 1, 1 };

            rotatePivot.localScale = new Vector3(faceFlip, 1, 1);

            Level = 1;
            
            UpdateGroupData();
            UpgradeData(true);
            
            if (!isCachedAudioClip)
            {
                audioClip = await AssetBundleManager.GetAssetCached<AudioClip>(weaponData.audioClip);
                isCachedAudioClip = true;
            }
            
            attackCoroutine = StartCoroutine(AutoAttackIE());
        }

        protected virtual void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        protected virtual void OnDestroy()
        {
            string audioPath = WeaponData.audioClip;
            
            if (!string.IsNullOrEmpty(audioPath)) AssetBundleManager.UnCache(audioPath);
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
                UpdateGroupData();
            }

            int idx = Array.IndexOf(groups, upgradeData);
            isUpgraded[idx] = true;

            levelUp.Increase(upgradeData);
            UpgradeData(false);
        }

        public void ChangeTimeScale(float timeScale, float deltaTime)
        {
            this.TimeScale = timeScale;
            this.DeltaTime = deltaTime;
        }

        public virtual void Pause(bool pause)
        {
            IsPaused = pause;

            if (pause)
            {
                OnPause();
            }
            else
            {
                OnResume();
            }
        }

        protected virtual void OnPause()
        {
            IsAttacking = false;
            StopCoroutine(attackCoroutine);
        }

        protected virtual void OnResume()
        {
            attackCoroutine = StartCoroutine(AutoAttackIE());
        }

        protected virtual IEnumerator AutoAttackIE()
        {
            while (!IsAttacking)
            {
                float cooldown = WeaponData.cooldown * (1 - CurrentUpgradeData.cooldownReduce);

                yield return new WaitForSeconds(cooldown / TimeScale);

                if (IsPaused || !IsActivated || IsAttacking)
                {
                    //Debug.LogError("stop attack (1)");
                    continue;
                }
                
                Vector3 position = GetMuzzlePosition();

                bool found = FindTarget(SkillData.findTarget, position);
                
                if (!found)
                {
                    //Debug.LogError("stop attack (2)");
                    continue;
                }
                
                yield return RotateIE(position, destination);
                
                if (IsPaused || !IsActivated || IsAttacking)
                {
                    //Debug.LogError("stop attack (3)");
                    continue;
                }
                
                OnPlayAttack();
                
                IsAttacking = true;
                
                yield break;
            }
        }

        protected virtual void OnPlayAttack()
        {
        }

        protected virtual void ExecuteAttack()
        {
            PlayAudioAttackOneShot();
            
            Vector3 muzzlePosition = GetMuzzlePosition();
           
            Vector3 destinationPosition = GetDestination(muzzlePosition, destination);

            SkillRuntimeData runtimeData = new SkillRuntimeData
            {
                Attack = (1 + CurrentUpgradeData.damagePercent) * WeaponData.attack,
                CritChance = CurrentUpgradeData.critChance + WeaponData.critChance,
                CritDamage = CurrentUpgradeData.critDamage + WeaponData.critDamage,
                ParallelCount = CurrentUpgradeData.parallelCount,
                ParallelDamagePercent = CurrentUpgradeData.parallelDamagePercent,
                SpreadCount = CurrentUpgradeData.spreadCount,
                SpreadDamagePercent = CurrentUpgradeData.spreadDamagePercent,
                PiercingCount = CurrentUpgradeData.piercingCount,
                ExplosiveRadius = CurrentUpgradeData.explosiveRadius,
                ExplosiveDamagePercent = CurrentUpgradeData.explosiveDamagePercent,
                BounceCount = CurrentUpgradeData.bounceCount,
                BounceDamagePercent = CurrentUpgradeData.bounceDamagePercent,
                KillInstantBelowHealthPercent = CurrentUpgradeData.killInstantBelowHealthPercent,
                Trajectory = trajectory,
                IsFlyWeapon = IsFlyWeapon,
                FlyWeapon = IsFlyWeapon ? this : null,
            };
            
            SkillManager.CastSkill(SkillData, runtimeData, muzzlePosition, destinationPosition, entity);
        }

        public virtual void OnStopAttack()
        {
            IsAttacking = false;
            
            StopCoroutine(attackCoroutine);
            
            attackCoroutine = StartCoroutine(AutoAttackIE());
        }
        
        protected abstract bool IsFlyWeapon { get; }
        
        protected virtual Vector3 GetMuzzlePosition() => muzzle.position;

        protected virtual Vector3 GetDestination(Vector3 @from, Vector3 @to) => @to;

        protected virtual void PlayAudioAttackOneShot()
        {
            SoundManager.Instance.PlayOneShot(audioClip, WeaponData.volume);
        }

        private IEnumerator RotateIE(Vector3 position, Vector3 target)
        {
            Vector3 direction = target - position;

            float angleFrom = rotatePivot.eulerAngles.z;
            float angleTo = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float angleDelta = Mathf.Abs(Mathf.DeltaAngle(angleFrom, angleTo));
            float dynamicDuration = Mathf.Lerp(0f, rotationDuration, angleDelta / 180f);

            float elapsedTime = 0;
            
            while (elapsedTime <= dynamicDuration)
            {
                elapsedTime += DeltaTime;
                
                float t = Mathf.Clamp01(elapsedTime / dynamicDuration);
                
                float s = rotationCurve.Evaluate(t);
                
                float a = Mathf.LerpAngle(angleFrom, angleTo, s);
                
                rotatePivot.eulerAngles = new Vector3(0, 0, a);

                yield return null;
            }

            rotatePivot.localScale = new Vector3(1, Mathf.Abs(angleTo) <= 90 ? 1 : -1f, 1);
        }

        protected bool FindTarget(FindTargetType type, Vector3 position)
        {
            Vector3 center = Vector3.zero;
            float radius = SkillData.findRadius;
            float sqrRadius = radius * radius;

            query.FindTarget(type, center, position, radius, (e, float2) =>
            {
                HealthData healthData = ComponentManager<HealthData>.Get(e);
                if (healthData.PredictedHealth <= 0)
                {
                    return false;
                }

                float d = math.lengthsq(float2);
                return d <= sqrRadius;
            }, out QueryResult result);

            entity = -1;
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

            return entity != -1;
        }

        private IEnumerator ChangeColorIE(Color targetColor)
        {
            float elapsedTime = 0f;

            renderer.GetPropertyBlock(propertyBlock);

            Color color = propertyBlock.GetColor(OutlineColorProperty);

            while (elapsedTime < adjustOutlineColorDuration)
            {
                elapsedTime += DeltaTime;

                float t = Mathf.Clamp01(elapsedTime / adjustOutlineColorDuration);

                propertyBlock.SetColor(OutlineColorProperty, Color.Lerp(color, targetColor, t));

                renderer.SetPropertyBlock(propertyBlock);

                yield return null;
            }
        }

        private void UpdateGroupData()
        {
            int[] idx;
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

        private void UpgradeData(bool updateOutline)
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
                if (adjustOutlineColorCoroutine != null) StopCoroutine(adjustOutlineColorCoroutine);
                adjustOutlineColorCoroutine = StartCoroutine(ChangeColorIE(color));
            }
        }
    }
}
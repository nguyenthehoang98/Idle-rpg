using System.Collections;
using System.Collections.Generic;
using _Game.Configs;
using _Game.GamePlay.Data;
using _Game.GamePlay.Manager;
using _Game.GamePlay.Utils;
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

        [SerializeField] private Transform muzzle;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform rotatePivot;
        [SerializeField] private AnimationCurve rotationCurve;
        [SerializeField] private float rotationDuration;

        private HashSet<string> names = new HashSet<string>();

        private SkillData skillData;
        private WeaponUpgradeData current;
        private WeaponUpgradeData levelUp;
        private WeaponUpgradeData powerX2;
        private WeaponUpgradeData powerX3;

        private IQuery query;
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
                level = value;
                isActivated = value > 0;
                RefreshUpgradeData();
            }
        }
        
        private void OnDestroy()
        {
            foreach (var assetName in names)
            {
                AssetBundleManager.UnCache(assetName);
            }

            names = null;
        }

        public async UniTask Initialize(WeaponData weaponData, WeaponUpgradeData upgradeDataX2, WeaponUpgradeData upgradeDataX3)
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
        }

        public void IncreaseUpgradeData(WeaponUpgradeData upgradeData)
        {
            levelUp.Increase(upgradeData);
            RefreshUpgradeData();
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
            
            SkillManager.CastSkill(skillData, runtimeData, muzzlePosition, destinationPosition);
        }

        public void EndAnimation()
        {
            isAttacking = false;
        }

        private void RefreshUpgradeData()
        {
            current = levelUp;
            if (level == 2) current.Increase(powerX2);
            else if (level == 3) current.Increase(powerX3);
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
                query.FindTarget(type, center, radius, (entity, float2) =>
                {
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

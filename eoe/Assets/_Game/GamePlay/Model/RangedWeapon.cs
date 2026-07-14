using System.Collections;
using System.Collections.Generic;
using _Game.Configs;
using _Game.GamePlay.Manager;
using _KITSystem.Resource;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game.GamePlay.Model
{
    public class RangedWeapon : BaseWeapon
    {
        private static readonly int AttackAnimator = Animator.StringToHash("Attack");
        
        [SerializeField] private Transform muzzle;
        [SerializeField] private Animator animator;

        private Coroutine autoAttackCoroutine;
        private AudioClip audioClip;
        private bool isCachedPrefab;
        private bool isCachedAudioClip;

        private bool isAttacking;
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            string prefabName = SkillData.prefabName;
            AssetBundleManager.UnCache(prefabName);
        }

        public override async UniTask Initialize(WeaponData weaponData, Dictionary<int, List<WeaponUpgradeData>> dict, WeaponUpgradeData upgradeDataX2,
            WeaponUpgradeData upgradeDataX3, float faceFlip)
        {
            if (!isCachedPrefab)
            {
                string prefabName = weaponData.skillData.prefabName;
                GameObject go = await AssetBundleManager.GetAssetCached<GameObject>(prefabName);
                Pool.RegisterPool(go, true);
                Pool.Destroy(Pool.Instantiate(go));
                isCachedPrefab = true;
            }

            if (!isCachedAudioClip)
            {
                audioClip = await AssetBundleManager.GetAssetCached<AudioClip>(weaponData.audioClip);
                isCachedAudioClip = true;
            }
            
            await base.Initialize(weaponData, dict, upgradeDataX2, upgradeDataX3, faceFlip);
            
            autoAttackCoroutine = StartCoroutine(AutoAttackIE());
        }

        private IEnumerator AutoAttackIE()
        {
            while (!IsPaused)
            {
                float cooldown = WeaponData.cooldown * (1 - CurrentUpgradeData.cooldownReduce);

                yield return new WaitForSeconds(cooldown / TimeScale);

                if (isAttacking || !IsActivated) continue;

                int entity;
                Vector3 position = GetMuzzlePosition();
                Vector3 destination;

                bool found = FindTarget(SkillData.findTarget, position, out entity, out destination);

                if (!found) continue;

                yield return RotateIE(position, destination);

                if (!IsActivated || IsPaused) continue;

                OnAttack();

                animator.Play(AttackAnimator, 0, 0);

                animator.speed = TimeScale * (WeaponData.attackSpeed + CurrentUpgradeData.attackSpeed);
                
                isAttacking = true;
            }
        }
        
        /*
         * @Abstract
         */

        protected virtual void OnAttack()
        {
        }
        
        protected virtual Vector3 GetMuzzlePosition() => muzzle.position;

        protected virtual Vector3 GetDestination(Vector3 @from, Vector3 @to) => @to;

        protected override void OnPause()
        {
            base.OnPause();
            StopCoroutine(autoAttackCoroutine);
            isAttacking = false;
        }

        protected override void OnResume()
        {
            base.OnResume();
            autoAttackCoroutine = StartCoroutine(AutoAttackIE());
        }
        
        /*
         * @private. callback animation
         */

        public void ExecuteAnimation()
        {
            if (!IsActivated || !isAttacking) return;
            
            SoundManager.Instance.PlayOneShot(audioClip, WeaponData.volume);
        }

        private void EndAnimation() => isAttacking = false;
    }
}
using System.Collections;
using _Game.Configs;
using _Game.GamePlay.SkillSystem;
using _Game.GamePlay.SoundSystem;
using _KITSystem.Resource;
using _KITSystem.SkillSystem.Core;
using UnityEngine;

namespace _Game.GamePlay
{
    public class Weapon : MonoBehaviour
    {
        private static readonly int Attack = Animator.StringToHash("Attack");

        [SerializeField] private Transform muzzle;
        [SerializeField] private Animator animator;
        [SerializeField] private float weaponRotationDuration = 0.15f;
        [SerializeField] private AnimationCurve weaponRotationCurve;

        private float TimeScale { get; set; } = 1;
        private float DeltaTime { get; set; } = 0.0334f;
        public bool IsActivated { private get; set; } = false;
        private SkillData SkillData { get; set; }
        private WeaponData WeaponData { get; set; }
        
        private Vector3 destination;
        private bool attacking;

        private void Awake()
        {
            DeltaTime = Time.deltaTime;
        }

        public void Initialize(WeaponData weaponData, float timeScale, float deltaTime)
        {
            WeaponData = weaponData;
            SkillData = weaponData.skillData;
            TimeScale = timeScale;
            DeltaTime = deltaTime;
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

                Vector3 position = muzzle != null ? muzzle.position : Vector3.zero;

                SkillTickable.FindTarget(type, new Vector2(position.x, position.y),
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
                    continue;
                }

                Vector3 direction = destination - position;

                float angleFrom = transform.eulerAngles.z;
                float angleTo = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                float elapsedTime = 0;

                float angleDelta = Mathf.Abs(Mathf.DeltaAngle(angleFrom, angleTo));

                float dynamicDuration = Mathf.Lerp(
                    0f,
                    weaponRotationDuration,
                    angleDelta / 180f
                );

                while (elapsedTime <= dynamicDuration)
                {
                    float dt = DeltaTime / TimeScale;

                    elapsedTime += dt;
                    float t = Mathf.Clamp01(elapsedTime / dynamicDuration);
                    float s = weaponRotationCurve.Evaluate(t);

                    float angle = Mathf.LerpAngle(angleFrom, angleTo, s);
                    transform.eulerAngles = new Vector3(0, 0, angle);

                    yield return new WaitForSeconds(dt);
                }

                transform.eulerAngles = new Vector3(0, 0, angleTo);
                animator.Play(Attack, 0, 0);
                animator.speed = TimeScale * WeaponData.attackSpeed;
                attacking = true;
                yield return null;
            }
        }

        private bool FilterEntity(int entity)
        {
            return true;
        }

        public async void ExecutePrivate()
        {
            if (attacking)
            {
                SkillTickable.CastSkill(SkillData, muzzle != null ? muzzle.position : Vector3.zero, destination);
                
                AudioClip clip = null;
                if (!string.IsNullOrEmpty(WeaponData.attackAudioClip))
                {
                    clip = await AssetBundleManager.GetAssetCached<AudioClip>(WeaponData.attackAudioClip);
                }
                SoundManager.Instance.PlayOneShot(clip, WeaponData.attackVolume);
                
                attacking = false;
            }
        }
    }
}
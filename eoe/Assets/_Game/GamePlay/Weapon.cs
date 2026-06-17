using System;
using System.Collections;
using System.Diagnostics;
using _Game.Configs;
using _Game.GamePlay.SkillSystem;
using _KITSystem.Config;
using _KITSystem.SkillSystem.Core;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace _Game.GamePlay
{
    public class Weapon : MonoBehaviour
    {
        private static readonly int ATTACK = Animator.StringToHash("Attack");

        public int skillId;
        public float cooldown = 0.1f;
        public float attackSpeed = 1.0f;

        [SerializeField] private Transform muzzle;
        [SerializeField] private new SpriteRenderer renderer;
        [SerializeField] private Animator animator;
        [SerializeField] private float weaponRotationDuration = 0.15f;
        [SerializeField] private AnimationCurve weaponRotationCurve;

        public float TimeScale { private get; set; } = 1;
        public float DeltaTime { private get; set; }
        public bool IsActivated { private get; set; } = false;

        private SkillData skillData;
        private Vector3 destination;
        private bool attacking;

        public Color Color
        {
            get => renderer.color;
            set => renderer.color = value;
        }
        
        private void Awake()
        {
            DeltaTime = Time.deltaTime;
        }

        private void Start()
        {
            bool found = ConfigManager.Get<SkillConfig>().TryGetSkill(skillId, out skillData);
            if (!found) Debug.LogError($"Skill '{skillId}' not found");

            StartCoroutine(AutoAttack());
        }

        private IEnumerator AutoAttack()
        {
            while (true)
            {
                yield return new WaitForSeconds(cooldown / TimeScale);

                if (attacking || !IsActivated) continue;

                FindTargetType type = FindTargetType.Filter;
                if(skillData.findTarget.type == FindTargetData.FilterType.Farthest)
                    type = FindTargetType.Farthest;
                else if (skillData.findTarget.type == FindTargetData.FilterType.Nearest)
                    type = FindTargetType.Nearest;
                
                Vector3 position = muzzle.position;

                SkillTickable.FindTarget(type, new Vector2(position.x, position.y),
                    skillData.findTarget.radius, FilterEntity, out var result);

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

                float dynamicDuration = Mathf.Lerp(0, weaponRotationDuration, Mathf.Abs(angleFrom - angleTo) / 180f);

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
                animator.Play(ATTACK, 0, 0);
                animator.speed = TimeScale * attackSpeed;
                attacking = true;
                yield return null;
            }
        }

        private bool FilterEntity(int entity)
        {
            return true;
        }

        public void ExecutePrivate()
        {
            if (attacking)
            {
                SkillTickable.CastSkill(skillData, muzzle.position, destination); 
                attacking = false;
            }
        }
    }
}
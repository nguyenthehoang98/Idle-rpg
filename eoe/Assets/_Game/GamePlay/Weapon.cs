using System;
using _Game.Configs;
using _Game.GamePlay.SkillSystem;
using _KITSystem.Config;
using UnityEngine;

namespace _Game.GamePlay
{
    public class Weapon : MonoBehaviour
    {
        private static readonly int ATTACK = Animator.StringToHash("Attack");

        public int skillId;
        [SerializeField] private Transform muzzle;
        [SerializeField] private new SpriteRenderer renderer;
        [SerializeField] private Animator animator;

        private SkillData skillData;
        private Vector3 destination;
        private bool attacking;

        public Color Color
        {
            get => renderer.color;
            set => renderer.color = value;
        }

        private void Start()
        {
            bool found = ConfigManager.Get<SkillConfig>().TryGetSkill(skillId, out skillData);
            if (!found) Debug.LogError($"Skill '{skillId}' not found");
        }

        public void Attack(Vector3 target)
        {
            attacking = true;
            destination = target;
            animator.Play(ATTACK);
        }

        public void ExecutePrivate()
        {
            if (attacking)
            {
                attacking = false;
                SkillTickable.CastSkill(skillData, muzzle.position, destination); 
            }
        }
        
        public float EulerAngleZ
        {
            get => transform.eulerAngles.z;
            set => transform.eulerAngles = new Vector3(0,0, value);
        }
    }
}
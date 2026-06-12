using _Echo.Scripts.AnimationSystem;
using _KITSystem.Utils;
using UnityEngine;

namespace _Echo.Scripts.Battle
{
    public class Character : MonoBehaviour
    {
        public Projectile projectile;
        public Vector3 muzzlePositionOffset;
        public float delayInitProjectile;
        
        [Header("References")]
        [SerializeField] private UnitAnimator animator;
        [SerializeField] private new UnitRenderer renderer;

        public UnitAnimator Animator => animator;

        private Direction direction;

        private void Awake()
        {
            animator.OnAnimationStart += AnimationStart;
        }
        
        private void Update()
        {
            int horizontal = (int)Input.GetAxisRaw("Horizontal");
            
            int vertical = (int)Input.GetAxisRaw("Vertical");

            bool isMoving = horizontal != 0 || vertical != 0;

            if (isMoving)
            {
                direction = GetDirection(horizontal, vertical);

                animator.Play(AnimState.Idle, direction);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                int result = animator.Play(AnimState.Attack, direction);
                if (result > 0)
                {
                    CastProjectile(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
                }
            }
        }
        
        private Direction GetDirection(int horizontal, int vertical)
        {
            if (horizontal == 0 && vertical == 1)
                return Direction.T;

            if (horizontal == 1 && vertical == 1)
                return Direction.TR;

            if (horizontal == 1 && vertical == 0)
                return Direction.R;

            if (horizontal == 1 && vertical == -1)
                return Direction.BR;

            if (horizontal == 0 && vertical == -1)
                return Direction.B;

            if (horizontal == -1 && vertical == -1)
                return Direction.BL;

            if (horizontal == -1 && vertical == 0)
                return Direction.L;

            if (horizontal == -1 && vertical == 1)
                return Direction.TL;

            return direction;
        }

        private void AnimationStart((Texture defaultTexture, Texture hdrTexture) tuple)
        {
            renderer.SetTexture(tuple.defaultTexture, tuple.hdrTexture, tuple.hdrTexture == null);
        }

        public void Activate(float duration)
        {
            animator.IsPaused = false;
            renderer.Activate(duration);
        }

        public void Deactivate(float duration)
        {
            animator.IsPaused = true;
            renderer.Deactivate(duration);
        }

        private void CastProjectile(Vector3 target)
        {
            Vector3 finalPosition = transform.position + muzzlePositionOffset;
            float duration = projectile.duration;
            Projectile p = null;
            this.WaitInvoke(delayInitProjectile, () =>
            {
                p = Instantiate(projectile, finalPosition, Quaternion.identity);
                p.SetDirection(target - finalPosition);
            });
            this.WaitInvoke(delayInitProjectile + duration, () =>
            {
                if(p != null) Object.Destroy(p.gameObject);
            });
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using _Games.GamePlay.AnimationSystem;
using _KITSystem.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Games.GamePlay
{
    public class Character : MonoBehaviour
    {
        public float cooldown = 2;
        
        [Header("Projectile")]
        public Projectile projectile;
        public Vector3 muzzlePositionOffset;
        public float delayInitProjectile;
        
        [Header("References")]
        [SerializeField] private new HDRRenderer renderer;


        private Queue<Vector3> queue = new Queue<Vector3>();

        private void Awake()
        {
            /*animator.OnAnimationStart += AnimationStart;
            animator.OnAnimationTrigger += state =>
            {
                if(state == State.Attack && queue.Count > 0) CastProjectile();
            };
            animator.OnAnimationEnd += state =>
            {
                if (state == State.Attack) animator.Play(State.Idle);
            };*/
        }

        private void Start()
        {
            StartCoroutine(AutoCastIE());
        }

        private IEnumerator AutoCastIE()
        {
            while (true)
            {
                yield return new WaitForSeconds(cooldown);

                Vector3 position = transform.position + muzzlePositionOffset;
                
                Monster[] monsters = Object.FindObjectsByType<Monster>(FindObjectsSortMode.None);
                
                float minDistance = float.MaxValue;
                
                Monster monster = null;
                
                foreach (var m in monsters)
                {
                    Vector3 p = m.transform.position;

                    float d = Vector3.Distance(position, p);

                    if (d < minDistance)
                    {
                        minDistance = d;
                        
                        monster = m;
                    }
                }

                if (monster != null)
                {
                    Vector3 destination = monster.transform.position;
                    
                    //animator.Play(State.Attack, position, destination);
                    
                    queue.Enqueue(destination);
                }
            }
        }

        private void AnimationStart((Texture defaultTexture, Texture hdrTexture) tuple)
        {
            renderer.SetTexture(tuple.defaultTexture, tuple.hdrTexture);
        }

        public void Activate(float duration)
        {
            //animator.IsPaused = false;
            renderer.Activate(duration);
        }

        public void Deactivate(float duration)
        {
            //animator.IsPaused = true;
            renderer.Deactivate(duration);
        }

        private void CastProjectile()
        {
            Vector3 position = transform.position + muzzlePositionOffset;
            Vector3 destination = queue.Dequeue();
            Projectile p = null;
            
            this.WaitInvoke(delayInitProjectile, () =>
            {
                p = Instantiate(projectile, position, Quaternion.identity);
                p.SetDestination(destination - position);
                Debug.DrawLine(position, destination, Color.yellow, 2);
            });
            
            this.WaitInvoke(delayInitProjectile + projectile.duration, () =>
            {
                if (p != null) Object.Destroy(p.gameObject);
            });
        }
    }
}
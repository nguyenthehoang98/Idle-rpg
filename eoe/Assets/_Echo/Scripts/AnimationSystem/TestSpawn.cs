using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Echo.Scripts.AnimationSystem
{
    public class TestSpawn : MonoBehaviour
    {
        public UnitAnimator animatorPrefab;
        public float stopDistance = 2.5f;
        
        private List<UnitAnimator> animators = new List<UnitAnimator>();
        private Stack<UnitAnimator> free = new Stack<UnitAnimator>();
        private Queue<UnitAnimator> queue = new Queue<UnitAnimator>();

        private System.Random rand = new System.Random();

        private void Start()
        {
            StartCoroutine(OnPostRender());
        }

        private void Update()
        {
            while (queue.Count > 0)
            {
                animators.Add(queue.Dequeue());
            }

            for (int i = animators.Count - 1; i >= 0; i--)
            {
                UnitAnimator animator = animators[i];

                if (animator == null)
                {
                    animators.RemoveAt(i);
                    continue;
                }
                
                Vector3 direction = -animator.transform.position;
                Vector3 position = animator.transform.position + direction.normalized * Time.deltaTime;
                animator.transform.position = position;

                if (position.magnitude < stopDistance)
                {
                    animators.RemoveAt(i);
                    animator.gameObject.SetActive(false);
                    free.Push(animator);
                }
            }
        }

        private IEnumerator OnPostRender()
        {
            while (true)
            {
                yield return new WaitForSeconds(rand.Next(1, 3));

                Vector3 normal = new Vector3(rand.Next(-100, 100) / 100f, rand.Next(-100, 100) / 100f).normalized;
                Vector3 position = normal * 10;

                UnitAnimator instance;
                if (free.Count > 0) instance = free.Pop();
                else instance = Instantiate(animatorPrefab, position, Quaternion.identity);

                instance.transform.position = position;
                instance.gameObject.SetActive(true);

                instance.Play(AnimState.Walk, position, Vector3.zero);
                
                queue.Enqueue(instance);
            }
        }
    }
}
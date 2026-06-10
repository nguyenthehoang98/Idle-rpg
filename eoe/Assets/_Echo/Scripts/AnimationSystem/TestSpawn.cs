using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Echo.Scripts.AnimationSystem
{
    public class TestSpawn : MonoBehaviour
    {
        public SpriteAnimator animatorPrefab;
        public float stopDistance = 2.5f;
        
        private List<SpriteAnimator> animators = new List<SpriteAnimator>();
        private Stack<SpriteAnimator> free = new Stack<SpriteAnimator>();
        private Queue<SpriteAnimator> queue = new Queue<SpriteAnimator>();

        private System.Random rand = new System.Random();

        private void Start()
        {
            StartCoroutine(AutoSpawnIE());
        }

        private void Update()
        {
            while (queue.Count > 0)
            {
                animators.Add(queue.Dequeue());
            }

            for (int i = animators.Count - 1; i >= 0; i--)
            {
                SpriteAnimator animator = animators[i];
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

        IEnumerator AutoSpawnIE()
        {
            while (true)
            {
                yield return new WaitForSeconds(rand.Next(1, 3) / 50f);

                Vector3 normal = new Vector3(rand.Next(-100, 100) / 100f, rand.Next(-100, 100) / 100f).normalized;
                Vector3 position = normal * 10;

                SpriteAnimator instance = null;
                if (free.Count > 0) instance = free.Pop();
                else instance = Instantiate(animatorPrefab, position, Quaternion.identity);

                instance.transform.position = position;
                instance.gameObject.SetActive(true);
                
                queue.Enqueue(instance);
            }
        }
    }
}
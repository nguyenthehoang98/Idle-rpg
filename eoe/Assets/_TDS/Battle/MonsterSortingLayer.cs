using System.Collections;
using UnityEngine;

namespace _TDS.Battle
{
    public class MonsterSortingLayer : MonoBehaviour
    {
        [SerializeField] private Transform scaleTransform;
        
        [Header("Renderers")]
        [SerializeField] private SpriteRenderer[] renderers;

        private Vector3 localScale;

        private void Awake()
        {
            localScale = scaleTransform.localScale;
        }

        private void OnEnable()
        {
            StartCoroutine(AutoSort());            
        }

        private IEnumerator AutoSort()
        {
            while (true)
            {
                int idy = Mathf.RoundToInt(-transform.position.y * 10);
                
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].sortingOrder = idy + i;
                }

                int flip = transform.position.x < 0 ? 1 : -1;
                
                scaleTransform.localScale = new Vector3(Mathf.Abs(localScale.x) * flip, localScale.y, localScale.z);
                
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
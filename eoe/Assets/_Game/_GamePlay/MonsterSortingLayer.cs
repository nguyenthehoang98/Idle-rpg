using System.Collections;
using UnityEngine;

namespace _Game._GamePlay
{
    public class MonsterSortingLayer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] renderers;
        
        private void OnEnable()
        {
            StartCoroutine(AutoSort());            
        }

        private IEnumerator AutoSort()
        {
            while (true)
            {
                int idx = Mathf.RoundToInt(-transform.position.y * 1000);
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].sortingOrder = idx + i;
                }
                
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
using System.Collections;
using UnityEngine;

namespace _Game.GamePlay.Model
{
    public class MonsterSortingLayer : MonoBehaviour
    {
        [SerializeField] private Transform rendererTransform;
        [SerializeField] private SpriteRenderer[] renderers;
        
        private void OnEnable()
        {
            StartCoroutine(AutoSort());            
        }

        private IEnumerator AutoSort()
        {
            while (true)
            {
                int idy = Mathf.RoundToInt(-transform.position.y * 1000);
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].sortingOrder = idy + i;
                }

                int x = transform.position.x < 0 ? 1 : -1;
                rendererTransform.localScale = new Vector3(x, 1, 1);
                
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
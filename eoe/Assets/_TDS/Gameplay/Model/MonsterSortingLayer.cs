using System;
using System.Collections;
using UnityEngine;

namespace _Game.GamePlay.Model
{
    public class MonsterSortingLayer : MonoBehaviour
    {
        [SerializeField] private Transform rendererTransform;
        [SerializeField] private SpriteRenderer[] renderers;

        private Vector3 localScale;

        private void Awake()
        {
            localScale = rendererTransform.localScale;
        }

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

                int flip = transform.position.x < 0 ? 1 : -1;
                
                rendererTransform.localScale = new Vector3(Mathf.Abs(localScale.x) * flip, localScale.y, localScale.z);
                
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
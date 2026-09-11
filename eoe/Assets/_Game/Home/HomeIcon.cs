using System;
using System.Collections;
using UnityEngine;

namespace _Game.Home
{
    public class HomeIcon : MonoBehaviour
    {
        [SerializeField] private HomeFlagIcon flag01, flag02, flag03;

        private Coroutine coroutine;

        public void PlayCurrent()
        {
            if (coroutine != null) StopCoroutine(coroutine);

            coroutine = StartCoroutine(Sequence(1));
        }

        private IEnumerator Sequence(int index)
        {
            float d1 = flag01.PlayWithStatus(GetStatus(1, index));

            yield return null;
            
            flag01.gameObject.SetActive(true);
            
            yield return new WaitForSeconds(d1);

            float d2 = flag02.PlayWithStatus(GetStatus(2, index));
            
            yield return null;
            
            flag02.gameObject.SetActive(true);
            
            yield return new WaitForSeconds(d2);
            
            float d3 = flag03.PlayWithStatus(GetStatus(3, index));    
        
            yield return null;

            flag03.gameObject.SetActive(true);
            
            yield return new WaitForSeconds(d3);
        }

        private static HomeFlagIcon.Status GetStatus(int i, int index)
        {
            if (i == index) return HomeFlagIcon.Status.Unlocked;
            if (i < index) return HomeFlagIcon.Status.Passed;
            return HomeFlagIcon.Status.Locked;
        }

        private void OnEnable()
        {
            flag01.gameObject.SetActive(false);
            flag02.gameObject.SetActive(false);
            flag03.gameObject.SetActive(false);
        }
    }
}
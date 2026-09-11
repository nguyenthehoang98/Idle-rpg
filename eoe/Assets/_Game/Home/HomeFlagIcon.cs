using System;
using LitMotion.Animation;
using UnityEngine;

namespace _Game.Home
{
    public class HomeFlagIcon : MonoBehaviour
    {
        public enum Status
        {
            Locked,
            Unlocked,
            Passed,
        }

        [SerializeField] private LitMotionAnimation animationLocked;
        [SerializeField] private LitMotionAnimation animationUnlocked;
        [SerializeField] private LitMotionAnimation animationPassed;

        public float PlayWithStatus(Status status)
        {
            switch (status)
            {
                case Status.Locked:
                    animationLocked.Play();
                    return animationLocked.Duration();
                
                case Status.Unlocked:
                    animationUnlocked.Play();
                    return animationUnlocked.Duration();
                
                case Status.Passed:
                    animationPassed.Play();
                    return animationPassed.Duration();
            }
            
            return 0f;
        }

        /*private void OnEnable()
        {
            Debug.LogError($"{name} enable [{Time.time:f2}]");
        }

        private void OnDisable()
        {
            Debug.LogError($"{name} disable [{Time.time:f2}]");
        }*/
    }
}
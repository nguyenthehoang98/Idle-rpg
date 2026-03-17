using System;
using System.Collections;
using UnityEngine;

namespace _KIT.Utils
{
    public static class CoroutineUtils
    {
        static IEnumerator WaitInvokeIE(float duration, Action action)
        {
            yield return new WaitForSeconds(duration);
            action?.Invoke();
        }
        
        static IEnumerator WaitNextFrameIE(Action action, int frame)
        {
            yield return new WaitForEndOfFrame();
            for (int i = 0; i < frame; i++)
            {
                yield return null;
            }
            action?.Invoke();
        } 

        public static Coroutine WaitNextFrame(this MonoBehaviour target, Action action, int frame = 1)
        {
            if(target != null)
                return target.StartCoroutine(WaitNextFrameIE(action, frame));
            return null;
        }

        public static Coroutine WaitInvoke(this MonoBehaviour target, float duration, Action action)
        {
            if(target != null)
                return target.StartCoroutine(WaitInvokeIE(duration, action));
            return null;
        }
    }
}
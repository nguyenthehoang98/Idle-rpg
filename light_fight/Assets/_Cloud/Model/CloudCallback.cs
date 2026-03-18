using System;

namespace _Cloud.Model
{
    public static class CloudCallback
    {
        public static void Invoke(Action action)
        {
            UnityMainThreadDispatcher.Instance.Enqueue(action);
        }
    }
}
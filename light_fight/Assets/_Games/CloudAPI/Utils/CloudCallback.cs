using System;

namespace CloudAPI.Utils
{
    public static class CloudCallback
    {
        public static void Invoke(Action action)
        {
            UnityMainThreadDispatcher.Instance.Enqueue(action);
        }
    }
}
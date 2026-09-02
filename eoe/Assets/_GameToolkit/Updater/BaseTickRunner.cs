using System.Collections.Generic;
using UnityEngine;

namespace _Toolkit.Updater
{
    public abstract class BaseTickRunner : MonoBehaviour
    {
        public abstract void Tick(float deltaTime);
    }

    public abstract class BaseTickRunner<T> : BaseTickRunner where T : ITickRunner
    {
        Queue<T> addition = new Queue<T>();
        Queue<T> remove = new Queue<T>();
        List<T> list = new List<T>();

        public override void Tick(float deltaTime)
        {
            while (addition.Count > 0)
            {
                T item = addition.Dequeue();
                OnAdd(item);
                list.Add(item);
            }

            for (var i = list.Count - 1; i >= 0; i--)
            {
                var item = list[i];
                if (item == null)
                {
                    list.RemoveAt(i);
                    continue;
                }
             
                item.Tick(deltaTime);
            }

            while (remove.Count > 0)
            {
                T item = remove.Dequeue();
                OnRemove(item);
                list.Remove(item);
            }
        }

        protected virtual void OnAdd(T item)
        {
        }

        protected virtual void OnRemove(T item)
        {
        }

        public void Add(T detector) => addition.Enqueue(detector);

        public void Remove(T detector) => remove.Enqueue(detector);
    }
}
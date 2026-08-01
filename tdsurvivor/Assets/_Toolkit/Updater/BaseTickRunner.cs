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
                list.Add(addition.Dequeue());
            }

            foreach (var on in list)
            {
                on.Tick(deltaTime);
            }

            while (remove.Count > 0)
            {
                list.Remove(remove.Dequeue());
            }
        }

        public void Add(T detector) => addition.Enqueue(detector);

        public void Remove(T detector) => remove.Enqueue(detector);
    }
}
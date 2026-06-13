using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Games.GamePlay.AnimationSystem;
using _KITSystem.Schedule;
using UnityEngine;

namespace _Games.GamePlay
{
    [Serializable]
    public class AnimationTickable : ITickable
    {
        private List<UnitAnimation> list = new List<UnitAnimation>();
        private Queue<UnitAnimation> additionalQueue = new Queue<UnitAnimation>();
        private Queue<UnitAnimation> removeQueue = new Queue<UnitAnimation>();

        private static AnimationTickable instance;

        public Task Initialize()
        {
            instance = this;
            
            return Task.CompletedTask;
        }

        public void Tick(float deltaTime)
        {
            while (additionalQueue.Count > 0)
            {
                list.Add(additionalQueue.Dequeue());
            }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                UnitAnimation unit = list[i];

                if (unit != null)
                {
                    unit.Tick(deltaTime);
                }
                else
                {
                    list.RemoveAt(i);
                }
            }

            while (removeQueue.Count > 0)
            {
                list.Remove(removeQueue.Dequeue());
            }
        }

        public void Dispose()
        {
            instance = null;
            list = null;
            removeQueue = null;
            additionalQueue = null;
        }

        public static void Add(UnitAnimation unitAnimation)
        {
            if (instance != null) instance.additionalQueue.Enqueue(unitAnimation);
            else
                Debug.LogError("Instance AnimationTickable is null");
        }

        public static void Remove(UnitAnimation unitAnimation)
        {
            if (instance != null) instance.removeQueue.Enqueue(unitAnimation);
            else
                Debug.LogError("Instance AnimationTickable is null");
        }
    }
}

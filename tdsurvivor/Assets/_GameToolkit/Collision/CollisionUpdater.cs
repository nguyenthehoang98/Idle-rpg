using System.Collections.Generic;
using _GameToolkit.Updater;
using UnityEngine;

namespace _GameToolkit.Collision
{
    internal sealed class CollisionUpdater : BaseUpdatable
    {
        static CollisionUpdater instance;

        private Queue<BaseCollision> additionQueue = new Queue<BaseCollision>();
        private Queue<BaseCollision> removeQueue = new Queue<BaseCollision>();
        private List<BaseCollision> onGoings = new List<BaseCollision>();

        public static CollisionUpdater Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject("CollisionUpdater").AddComponent<CollisionUpdater>();
                }

                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
            }
        }

        public override void Tick(float deltaTime)
        {
            while (additionQueue.Count > 0)
            {
                onGoings.Add(additionQueue.Dequeue());
            }

            foreach (var on in onGoings)
            {
                on.Tick();
            }

            while (removeQueue.Count > 0)
            {
                onGoings.Remove(removeQueue.Dequeue());
            }
        }

        public void Add(BaseCollision collision) => additionQueue.Enqueue(collision);
        
        public void Remove(BaseCollision collision) => removeQueue.Enqueue(collision);
    }
}
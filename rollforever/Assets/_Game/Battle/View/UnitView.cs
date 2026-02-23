using System.Collections.Generic;
using _Game.Battle.Model;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle.View
{
    public class UnitView : MonoBehaviour
    {
        static Dictionary<int, UnitView> container = new Dictionary<int, UnitView>();
        
        private float elapsed;
        private float frameDeltaTime;
        private Vector2 prevPosition;
        private Vector2 currentPosition;
        private bool alive;
        
        private bool shouldDestroy;

        public static void TryUpdatePosition(int entity, float2 position)
        {
            if (container.TryGetValue(entity, out UnitView unitView))
            {
                unitView.UpdatePosition(position);
            }
        }

        public static bool TryRelease(int entity, out UnitView unitView)
        {
            return container.Remove(entity, out unitView);
        }
        
        public void Init(int entity, float2 startPosition, BattleStartupShareData shareData)
        {
            container[entity] = this;
            this.frameDeltaTime = shareData.TimeDelta;
            this.transform.position = this.prevPosition = this.currentPosition = startPosition;
            this.alive = true;
        }
        
        void UpdatePosition(Vector2 pos)
        {
            prevPosition = currentPosition;
            currentPosition = pos;
            elapsed = 0;
        }

        private void FixedUpdate()
        {
            elapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsed / frameDeltaTime);
            transform.position = Vector2.Lerp(prevPosition, currentPosition, t);
            if (shouldDestroy && t >= 1 && alive)
            {
                shouldDestroy = false;
                alive = false;
            }
        }
    }
}
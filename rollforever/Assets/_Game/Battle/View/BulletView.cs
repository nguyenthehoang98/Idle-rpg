using _Game.Battle.Model;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle.View
{
    public class BulletView : MonoBehaviour
    {
        private float elapsed;
        private float frameDeltaTime;
        private Vector2 prevPosition;
        private Vector2 currentPosition;
        private bool alive;
        
        private bool shouldDestroy;
        
        public void Init(float2 startPosition, BattleStartupShareData shareData)
        {
            frameDeltaTime = shareData.TimeDelta;
            transform.position = prevPosition = currentPosition = startPosition;
            alive = true;
        }
        
        public void UpdatePosition(Vector2 pos)
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
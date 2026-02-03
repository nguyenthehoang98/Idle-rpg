using UnityEngine;

namespace _Game.Battle.View
{
    public class UnitView : MonoBehaviour
    {
        private float elapsed;
        private float frameDeltaTime;
        private Vector2 prevPosition;
        private Vector2 currentPosition;
        private bool alive;
        
        private bool shouldDestroy;
        
        public void Init(BattleStartupShareData shareData)
        {
            this.frameDeltaTime = shareData.TimeDelta;
            this.prevPosition = this.currentPosition = transform.position;
            this.alive = true;
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
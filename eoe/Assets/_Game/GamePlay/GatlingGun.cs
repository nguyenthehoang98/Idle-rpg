using _KITSystem.Utils;
using UnityEngine;

namespace _Game.GamePlay
{
    public class GatlingGun : Weapon
    {
        [SerializeField] private float angleRandom = 15;
        [SerializeField] private Transform[] muzzles;

        private int muzzleIndex;
        private bool shouldRandom;

        protected override Vector3 GetDestination(Vector3 from, Vector3 to)
        {
            if (!shouldRandom)
            {
                shouldRandom = true;
                return to;
            }
            
            float randomAngle = Random.Range(-angleRandom, angleRandom);

            Vector3 dir = to - from;
            float distance = dir.magnitude;

            dir = Quaternion.Euler(0, 0, randomAngle) * dir.normalized;

            return from + dir * distance;
        }

        protected override void OnPlay()
        {
            shouldRandom = false;
        }

        protected override void OnExecute()
        {
            muzzleIndex++;
            
            if(muzzleIndex >= muzzles.Length) muzzleIndex = 0;
        }

        protected override Vector3 MuzzlePosition()
        {
            return muzzles[muzzleIndex].position;
        }
    }
}
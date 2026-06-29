using System;
using _KITSystem.Resource;
using _KITSystem.Utils;
using UnityEngine;

namespace _Game.GamePlay.View
{
    public class AutoDestroyGameObject : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 1f;

        private void OnEnable()
        {
            this.WaitInvoke(lifeTime, () => { Pool.Destroy(gameObject); });
        }
    }
}
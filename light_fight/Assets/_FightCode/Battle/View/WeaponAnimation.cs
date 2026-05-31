using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

namespace _FightCode.Battle.View
{
    public class WeaponAnimation : MonoBehaviour
    {
        [TitleGroup("Animation")]
        [SerializeField] private Transform muzzle;  
        [SerializeField] private SortingGroup sortingGroup;

        private void OnValidate()
        {
            //muzzle = transform.Find("Muzzle");
        }

        public void PlayAttack(float timeScale)
        {
        }

        public void Stop()
        {
        }

        public void Activate()
        {
            sortingGroup.sortingOrder = 1;
        }

        public void Deactivate()
        {
            sortingGroup.sortingOrder = 0;
        }
        
        public Vector3 MuzzlePosition => muzzle.position;
    }
}
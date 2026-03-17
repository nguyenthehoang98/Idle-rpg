using System;
using Unity.Mathematics;
using UnityEngine;

namespace _Games.Combat.Level
{
    public class LevelDesign : MonoBehaviour
    {
        [SerializeField, Tooltip("Các ví trí để đặt vũ khí")] 
        private SlotView[] slots;
        [SerializeField, Tooltip("Kích thước của từng ô")]
        private Vector2 cellSize = new Vector2(1f, 1f);
        [SerializeField] private CastleTurret turret;

        public event Action<int> OnTriggerWeapon;
        
        private void Start()
        {
            Transform[] temps = new Transform[slots.Length];
            for (int i = 0; i < slots.Length; i++)
            {
                temps[i] = slots[i].transform;
            }

            turret.Init(6, temps, slot =>
            {
                OnTriggerWeapon?.Invoke(slot);
            });
        }

        private void OnDrawGizmos()
        {
            float radius = 0.5f;
            Vector3 center = Vector3.zero;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(center + new Vector3(-radius, radius), center + new Vector3(radius, -radius));
            Gizmos.DrawLine(center + new Vector3(-radius, -radius), center + new Vector3(radius, radius));
        }

        public Vector2 CellSize => cellSize;

        public Vector2[] LoopPoints()
        {
            Vector2[] points = new Vector2[slots.Length];
            for (int i = 0; i < slots.Length; i++)
            {
                Vector2 point = slots[i].transform.position;
                points[i] = point;
            }

            return points;
        }

        public SlotView[] Slots => slots;
    }
}
using System;
using ProjectDawn.Navigation.Hybrid;
using UnityEngine;

namespace _Games.Combat.Level
{
    public class LevelDesign : MonoBehaviour
    {
        [SerializeField] private AgentCircleShapeAuthoring shapeAuthoring;
        [SerializeField, Tooltip("Các ví trí để đặt vũ khí")] 
        private SlotItem[] slots;
        [SerializeField, Tooltip("Kích thước của từng ô")]
        private Vector2 cellSize = new Vector2(1f, 1f);
        [SerializeField] private CastleTurret turret;

        public event Action<int> OnTriggerWeapon;

        public void Initialize(int totalRotate)
        {
            Transform[] temps = new Transform[slots.Length];
            for (int i = 0; i < slots.Length; i++)
            {
                temps[i] = slots[i].transform;
            }

            turret.Initialize(totalRotate, temps, slot =>
            {
                OnTriggerWeapon?.Invoke(slot);
            });
        }

        private void OnDrawGizmos()
        {
            float r = 0.5f;
            Vector3 center = Vector3.zero;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(center + new Vector3(-r, r), center + new Vector3(r, -r));
            Gizmos.DrawLine(center + new Vector3(-r, -r), center + new Vector3(r, r));
        }

        public float Radius => shapeAuthoring.Radius;

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

        public SlotItem[] Slots => slots;
    }
}
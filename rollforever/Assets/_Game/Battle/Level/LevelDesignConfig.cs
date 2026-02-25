using _Game.Battle.UI;
using UnityEngine;

namespace _Game.Battle.Level
{
    public class LevelDesignConfig : MonoBehaviour
    {
        [SerializeField] private Transform[] children;
        [SerializeField] private Transform[] slots;

        public Vector2[] LoopPoints()
        {
            Vector2[] points = new Vector2[children.Length];
            for (int i = 0; i < children.Length; i++)
            {
                points[i] = children[i].position;
            }

            return points;
        }

        public EquipmentItem[] AllEquipments()
        {
            EquipmentItem[] equipments = new EquipmentItem[slots.Length];
            for (int i = 0; i < equipments.Length; i++)
            {
                equipments[i] = slots[i].GetComponentInChildren<EquipmentItem>();
            }
            return equipments;
        }
    }
}
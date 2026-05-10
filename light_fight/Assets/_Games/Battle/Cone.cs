using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace _Games.Battle
{
    public class Cone : MonoBehaviour
    {
        [SerializeField] private MMF_Player zoomOutFeedback;
        [SerializeField] private MMF_Player zoomInFeedback;

        private List<int> dicesId = new List<int>();

        public void Init(int order)
        {
            transform.localRotation = Quaternion.Euler(0, 0, -60 * order);
        }

        public void Active() => zoomInFeedback.PlayFeedbacks();

        public void Inactive() => zoomOutFeedback.PlayFeedbacks();
        
        public void InsertId(int dice) => dicesId.Add(dice);
        
        public int Stack => dicesId.Count;
        
        public void RemoveId(int dice) => dicesId.Remove(dice);
    }
}
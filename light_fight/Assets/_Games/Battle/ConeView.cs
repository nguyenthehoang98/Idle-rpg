using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Games.Battle
{
    public class ConeView : MonoBehaviour
    {
        [SerializeField] private Transform pivot;
        [SerializeField] private StarView[] stars;
        [TitleGroup("Feedback")] 
        public MMF_Player initFeedback;
        public MMF_Player playFeedback;

        private void Awake()
        {
            pivot.gameObject.SetActive(false);
        }
    }
}
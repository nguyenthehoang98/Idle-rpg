using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Games.Battle
{
    public class Weapon : MonoBehaviour
    {
        [TitleGroup("Feedback")] 
        [SerializeField] private MMF_Player activeFeedback;
        [SerializeField] private MMF_Player inactiveFeedback;

        public void Active() => activeFeedback.PlayFeedbacks();
        public void Inactive() => inactiveFeedback.PlayFeedbacks();
    }
}
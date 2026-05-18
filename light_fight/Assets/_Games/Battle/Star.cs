using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Games.Battle
{
    public class Star : MonoBehaviour
    {
        [TitleGroup("Settings")]
        [SerializeField] private float delayActive = 0.2f;
        [SerializeField] private float delayInactive = 0.2f;
        [TitleGroup("Feedback")] 
        [SerializeField] private MMF_Player playFeedback;
        [SerializeField] private MMF_Player activeFeedback;
        [SerializeField] private MMF_Player inactiveFeedback;

        public float DelayActive => delayActive;

        public float DelayInactive => delayInactive;

        public float Play()
        {
            playFeedback.PlayFeedbacks();
            return playFeedback.TotalDuration;
        }

        public float Active()
        {
            Debug.Log("activeFeedback");
            activeFeedback.PlayFeedbacks();
            return activeFeedback.TotalDuration;
        }

        public float Inactive()
        {
            inactiveFeedback.PlayFeedbacks();
            return inactiveFeedback.TotalDuration;
        }
    }
}
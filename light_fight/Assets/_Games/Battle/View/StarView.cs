using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Games.Battle.View
{
    public class StarView : MonoBehaviour
    {
        [TitleGroup("Feedback")] 
        [SerializeField] private MMF_Player playFeedback;
        [SerializeField] private MMF_Player activeFeedback;
        [SerializeField] private MMF_Player inactiveFeedback;
        
        public float Play(float timeScale)
        {
            playFeedback.TimescaleMultiplier = timeScale;
            playFeedback.PlayFeedbacks();
            return playFeedback.TotalDuration;
        }

        public float Active(float timeScale)
        {
            activeFeedback.TimescaleMultiplier = timeScale;
            activeFeedback.PlayFeedbacks();
            return activeFeedback.TotalDuration;
        }
        
        public float Inactive(float timeScale)
        {
            inactiveFeedback.TimescaleMultiplier = timeScale;
            inactiveFeedback.PlayFeedbacks();
            return inactiveFeedback.TotalDuration;
        }
    }
}
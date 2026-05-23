using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Games.Battle
{
    public class StarView : MonoBehaviour
    {
        [TitleGroup("Feedback")] 
        [SerializeField] private MMF_Player playFeedback;
        [SerializeField] private MMF_Player activeFeedback;
        [SerializeField] private MMF_Player inactiveFeedback;
    }
}
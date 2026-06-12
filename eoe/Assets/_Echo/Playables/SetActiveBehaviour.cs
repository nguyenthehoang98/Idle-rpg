using UnityEngine.Playables;

namespace _Echo.Playables
{
    public class SetActiveBehaviour : PlayableBehaviour
    {
        public bool active;

        public override void OnBehaviourPlay(
            Playable playable,
            FrameData info)
        {
            var director =
                playable.GetGraph().GetResolver() as PlayableDirector;

            if (director != null)
            {
                director.gameObject.SetActive(active);
            }
        }
    }
}
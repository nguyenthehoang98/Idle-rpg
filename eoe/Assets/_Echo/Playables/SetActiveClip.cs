using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace _Echo.Playables
{
    [System.Serializable]
    public class SetActiveClip : PlayableAsset, ITimelineClipAsset
    {
        public bool active;

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(
            PlayableGraph graph,
            GameObject owner)
        {
            return Playable.Create(graph);
        }
    }
}
using UnityEngine;
using UnityEngine.Timeline;

namespace _Echo.Playables
{
    [TrackClipType(typeof(SetActiveClip))]
    [TrackBindingType(typeof(GameObject))]
    public class SetActiveTrack : TrackAsset
    {
    }
}
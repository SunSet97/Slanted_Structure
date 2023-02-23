using UnityEngine.Timeline;

namespace Utility.Cinematic.CustomTimeline
{
    [TrackColor(0.3f, 0.3f, 0.7f)]

    [TrackBindingType(typeof(CinematicCharacter))]
    [TrackClipType(typeof(TimelineCharClip))]
    public class TimelineCharAsset : TrackAsset
    {
    }
}

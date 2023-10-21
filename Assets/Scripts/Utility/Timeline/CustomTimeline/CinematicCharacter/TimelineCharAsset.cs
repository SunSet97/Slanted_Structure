using UnityEngine.Timeline;

namespace Utility.Timeline.CustomTimeline.CinematicCharacter
{
    [TrackColor(0.3f, 0.3f, 0.7f)]

    [TrackBindingType(typeof(Timeline.CinematicCharacter))]
    [TrackClipType(typeof(TimelineCharClip))]
    public class TimelineCharAsset : TrackAsset
    {
    }
}

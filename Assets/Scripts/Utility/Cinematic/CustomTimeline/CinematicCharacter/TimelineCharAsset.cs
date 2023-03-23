using UnityEngine.Timeline;

namespace Utility.Cinematic.CustomTimeline.CinematicCharacter
{
    [TrackColor(0.3f, 0.3f, 0.7f)]

    [TrackBindingType(typeof(Cinematic.CinematicCharacter))]
    [TrackClipType(typeof(TimelineCharClip))]
    public class TimelineCharAsset : TrackAsset
    {
    }
}

using UnityEngine.Timeline;

namespace Utility.Cinematic.CustomTimeline.Character
{
    [TrackColor(0.3f, 0.3f, 0.7f)]

    [TrackBindingType(typeof(CharacterBindingHelper))]
    [TrackClipType(typeof(CharClip))]
    public class CharAsset : TrackAsset
    {
    }
}

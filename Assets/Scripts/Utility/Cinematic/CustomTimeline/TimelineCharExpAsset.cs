using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Utility.Cinematic;

[TrackColor(0, 1, 0)]

[TrackBindingType(typeof(CinematicCharacter))]
[TrackClipType(typeof(TimelineCharExpClip))]
public class TimelineCharExpAsset : TrackAsset
{
}

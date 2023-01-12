using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Utility.Cinematic;

[TrackColor(0.3f, 0.3f, 0.7f)]

[TrackBindingType(typeof(CinematicCharacter))]
[TrackClipType(typeof(TimelineCharClip))]
public class TimelineCharAsset : TrackAsset
{
}

using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class TimelineCharExpClip : PlayableAsset, ITimelineClipAsset
{
    [SerializeField]
    private TimelineCharExpBehavior template = new TimelineCharExpBehavior();
        
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        return ScriptPlayable<TimelineCharExpBehavior>.Create(graph, template);
    }

    public ClipCaps clipCaps => ClipCaps.All;
}

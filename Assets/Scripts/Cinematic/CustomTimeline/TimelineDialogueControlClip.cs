using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class TimelineDialogueControlClip : PlayableAsset, ITimelineClipAsset
{
    [SerializeField]
    private TimelineDialogueControllerBehaviour template = new TimelineDialogueControllerBehaviour();
    
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        return ScriptPlayable<TimelineDialogueControllerBehaviour>.Create(graph, template);
    }

    public ClipCaps clipCaps => ClipCaps.All;
    
    
}

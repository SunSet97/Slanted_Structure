using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Utility.Cinematic.CustomTimeline.CinematicDialogue
{
    [Serializable]
    public class TimelineDialogueClip : PlayableAsset, ITimelineClipAsset
    {
        public TimelineDialogueBehaviour template = new TimelineDialogueBehaviour();

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<TimelineDialogueBehaviour>.Create(graph, template);
        }

        public ClipCaps clipCaps => ClipCaps.All;
    }
}

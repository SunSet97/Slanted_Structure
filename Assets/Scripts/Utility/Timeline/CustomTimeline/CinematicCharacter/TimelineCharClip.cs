using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Utility.Timeline.CustomTimeline.CinematicCharacter
{
    [Serializable]
    public class TimelineCharClip : PlayableAsset, ITimelineClipAsset
    {
        [SerializeField]
        private TimelineCharBehavior template = new TimelineCharBehavior();
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<TimelineCharBehavior>.Create(graph, template);
        }

        public ClipCaps clipCaps => ClipCaps.All;
    }
}

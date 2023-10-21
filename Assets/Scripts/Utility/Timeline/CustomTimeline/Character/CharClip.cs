using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Utility.Timeline.CustomTimeline.Character
{
    [Serializable]
    public class CharClip : PlayableAsset, ITimelineClipAsset
    {
        [SerializeField] private CharBehavior template = new CharBehavior();
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<CharBehavior>.Create(graph, template);
        }

        public ClipCaps clipCaps => ClipCaps.All;
    }
}

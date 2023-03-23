using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Utility.Cinematic.CustomTimeline.CinematicCharacter;

namespace Utility.Cinematic.CustomTimeline.Character
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

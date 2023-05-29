﻿using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Utility.Core;

namespace Utility.Cinematic.CustomTimeline.CinematicDialogue
{
    [TrackColor(1f, 1f, 0)]
    [TrackClipType(typeof(TimelineDialogueClip))]
    [TrackBindingType(typeof(DialogueController))]
    public class DialogueTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var clips = GetClips();
            foreach (var clip in clips)
            {
                var dialogueBehaviour = ((TimelineDialogueClip)clip.asset).template;
                dialogueBehaviour.UpdateTrack();
                clip.duration = dialogueBehaviour.GetDuration();
            }

            return base.CreateTrackMixer(graph, go, inputCount);
        }

        protected override void OnCreateClip(TimelineClip clip)
        {
            var dialogueBehaviour = ((TimelineDialogueClip)clip.asset).template;
            dialogueBehaviour.Init();
            base.OnCreateClip(clip);
        }
    }
}

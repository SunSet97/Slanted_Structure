using System;
using UnityEngine;
using UnityEngine.Playables;
using Utility.Core;

namespace Utility.Cinematic.CustomTimeline
{
    [Serializable]
    public class TimelineDialogueControllerBehaviour : PlayableBehaviour
    {
        public TextAsset dialogueJson;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (!Application.isPlaying || dialogueJson == null) return;
            playable.GetGraph().Stop();
            DialogueController.Instance.StartConversation(dialogueJson.text);
            DialogueController.Instance.SetDialougueEndAction(() => { playable.GetGraph().Play(); });
            dialogueJson = null;
        }
    }
}

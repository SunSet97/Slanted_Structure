using System;
using UnityEngine;
using UnityEngine.Playables;
using Utility.System;

[Serializable]
public class TimelineDialogueControllerBehaviour : PlayableBehaviour
{

    public TextAsset dialogueJson;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (dialogueJson == null) return;
        playable.GetGraph().Stop();
        DialogueController.instance.StartConversation(dialogueJson.text);
        DialogueController.instance.SetDialougueEndAction(() => { playable.GetGraph().Play(); });
        dialogueJson = null;
    }
}

using System;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class TimelineDialogueControllerBehaviour : PlayableBehaviour
{

    public TextAsset dialogueJson;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (dialogueJson == null) return;

        playable.GetGraph().Stop();
        CanvasControl.instance.StartConversation(dialogueJson.text);
        CanvasControl.instance.SetDialougueEndAction(() =>
        {
            playable.GetGraph().Play();
        });
        dialogueJson = null;
    }
}

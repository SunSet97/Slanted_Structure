using System;
using System.Linq;
using Data;
using UnityEngine;
using UnityEngine.Playables;
using Utility.Core;
using Utility.Property;

namespace Utility.Cinematic.CustomTimeline.CinematicDialogue
{
    [Serializable]
    public class TimelineDialogueBehaviour : PlayableBehaviour
    {
        public TextAsset dialogueJson;

        [SerializeField] private float dialoguePrintSec;
        [SerializeField] private bool isAuto;

        [ConditionalHideInInspector("isAuto")] [SerializeField]
        private float nextSec;

        [ConditionalHideInInspector("isAuto")] [SerializeField]
        private DialogueData dialogueData;

        private float duration;
        private DialogueController dialogueController;

        public void Init()
        {
            if (Application.isPlaying)
            {
                if (Mathf.Approximately(dialoguePrintSec, 0))
                {
                    dialoguePrintSec = DataController.Instance.dialoguePrintSec;
                }
            }
            else
            {
                dialoguePrintSec = 0.05f;
            }

            nextSec = 0.05f;
        }

        public void UpdateTrack()
        {
            if (!isAuto)
            {
                dialogueData = null;
                duration = 1f;
            }
            else
            {
                if (dialogueData.dialogues.Length == 0)
                {
                    dialogueData.Init(dialogueJson.text, dialoguePrintSec, nextSec);
                }
                else
                {
                    foreach (var dialogue in dialogueData.dialogues)
                    {
                        if (Mathf.Approximately(dialogue.PrintSec, dialogueData.DialoguePrintSec))
                        {
                            dialogue.PrintSec = dialoguePrintSec;
                        }

                        if (Mathf.Approximately(dialogue.NextSec, dialogueData.NextSec))
                        {
                            dialogue.NextSec = nextSec;
                        }
                    }
                }

                dialogueData.DialoguePrintSec = dialoguePrintSec;
                dialogueData.NextSec = nextSec;

                duration = 0f;

                foreach (var dialogue in dialogueData.dialogues)
                {
                    var prev = duration;
                    var len = dialogue.contents.Length - 1 > 0
                        ? dialogue.contents.Length - 1
                        : dialogue.contents.Length;
                    duration += dialogue.PrintSec * len + dialogue.NextSec;
                    dialogue.startTime = prev;
                    dialogue.endTime = duration;
                }
            }
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (!Application.isPlaying || dialogueJson == null)
            {
                return;
            }

            if (!isAuto)
            {
                playable.GetGraph().Stop();
                DialogueController.Instance.StartConversation(dialogueJson.text);
                DialogueController.Instance.SetDialogueEndAction(() => { playable.GetGraph().Play(); });
                dialogueJson = null;
            }
            else
            {
                if (Application.isPlaying)
                {
                    DialogueController.Instance.SetInputEnable(false);
                    DialogueController.Instance.dialoguePanel.SetActive(true);
                }
                else
                {
                    dialogueController.SetInputEnable(false);
                    dialogueController.dialoguePanel.SetActive(true);
                }
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            base.OnBehaviourPause(playable, info);
            if (!isAuto)
            {
                return;
            }

            if (Application.isPlaying)
            {
                DialogueController.Instance.SetInputEnable(true);
                DialogueController.Instance.dialoguePanel.SetActive(false);
            }
            else
            {
                dialogueController.SetInputEnable(true);
                dialogueController.dialoguePanel.SetActive(false);
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);

            if (!isAuto)
            {
                return;
            }

            var time = playable.GetTime();
            var dialogue = dialogueData.dialogues.First(item =>
                item.startTime <= playable.GetTime() && item.endTime >= playable.GetTime());

            if (!Application.isPlaying)
            {
                dialogueController = playerData as DialogueController;
                dialogueController.dialoguePanel.SetActive(true);
                var dur = Math.Min(Math.Max(time - dialogue.startTime, 0),
                    dialogue.endTime - dialogue.NextSec - dialogue.startTime);
                var contentCount = (int)(dur / dialogue.PrintSec) + 1;
                var content = dialogue.contents.Substring(0, contentCount);
                dialogueController.SetText(dialogue.name, content);
            }
            else
            {
                var dur = Math.Min(Math.Max(time - dialogue.startTime, 0),
                    dialogue.endTime - dialogue.NextSec - dialogue.startTime);
                var contentCount = (int)(dur / dialogue.PrintSec) + 1;
                var content = dialogue.contents.Substring(0, contentCount);
                DialogueController.Instance.SetText(dialogue.name, content);
            }
        }

        public float GetDuration()
        {
            return duration;
        }
    }
}
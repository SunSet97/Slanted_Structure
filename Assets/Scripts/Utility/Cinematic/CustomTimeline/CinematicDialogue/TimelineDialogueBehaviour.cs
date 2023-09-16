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
        public bool isAuto;

        [ConditionalHideInInspector("isAuto")] [SerializeField]
        private float nextSec;

        [ConditionalHideInInspector("isAuto")] [SerializeField]
        private bool isHold;

        [ConditionalHideInInspector("isAuto")] [SerializeField]
        private DialogueData dialogueData;

        private double clipStartTime;
        private double duration;
        private DialogueController dialogueController;

        public void Init()
        {
            if (Mathf.Approximately(dialoguePrintSec, 0))
            {
                dialoguePrintSec = Application.isPlaying ? DataController.Instance.dialoguePrintSec : 0.05f;
            }

            if (Mathf.Approximately(nextSec, 0))
            {
                nextSec = Application.isPlaying ? DataController.Instance.dialogueNextSec : 0.05f;
            }
        }

        public void UpdateTrack(string clipName, double clipStartTime)
        {
            this.clipStartTime = clipStartTime;
            
            if (!isAuto)
            {
                dialogueData = null;
                //duration = 1f;
            }
            else
            {
                // 기본값으로 초기화
                if (dialogueData.dialogues.Length == 0)
                {
                    dialogueData.Init(dialogueJson.text, dialoguePrintSec, nextSec);
                }
                else
                {
                    // 기본 세팅이 안되어있는 경우 - Clip Property 값으로 세팅
                    foreach (var dialogue in dialogueData.dialogues)
                    {
                        if (Mathf.Approximately(dialogue.printSec, 0))
                        {
                            dialogue.printSec = dialoguePrintSec;
                        }

                        if (Mathf.Approximately(dialogue.nextSec, 0))
                        {
                            dialogue.nextSec = nextSec;
                        }
                    }
                }

                duration = 0f;

                foreach (var dialogue in dialogueData.dialogues)
                {
                    var prev = duration;
                    var len = dialogue.contents.Length - 1 > 0
                        ? dialogue.contents.Length - 1
                        : dialogue.contents.Length;
                    duration += dialogue.printSec * len + dialogue.nextSec;
                    dialogue.startTime = prev;
                    dialogue.endTime = duration;
                    Debug.Log($"{clipName} Start - {prev},  EndTIme - {duration}, - {dialogue.printSec}, {len}, {dialogue.nextSec}");
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
                if (dialogueController)
                {
                    dialogueController.SetInputEnable(true);
                    dialogueController.dialoguePanel.SetActive(false);
                }
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
                item.startTime <= time && item.endTime >= time);

            if (!Application.isPlaying)
            {
                dialogueController = playerData as DialogueController;
                if (dialogueController)
                {
                    dialogueController.dialoguePanel.SetActive(true);
                    var dur = Math.Min(Math.Max(time - dialogue.startTime, 0),
                        dialogue.endTime - dialogue.nextSec - dialogue.startTime);
                    var contentCount = (int)(dur / dialogue.printSec) + 1;
                    var content = dialogue.contents.Substring(0, contentCount);
                    dialogueController.SetText(dialogue.name, content);
                }
            }
            else
            {
                var dur = Math.Min(Math.Max(time - dialogue.startTime, 0),
                    dialogue.endTime - dialogue.nextSec - dialogue.startTime);
                var contentCount = (int)(dur / dialogue.printSec) + 1;
                var content = dialogue.contents.Substring(0, contentCount);
                DialogueController.Instance.SetText(dialogue.name, content);
            }
        }

        public double GetDuration()
        {
            return duration;
        }
    }
}
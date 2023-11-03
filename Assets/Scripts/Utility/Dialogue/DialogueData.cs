using System;
using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Utility.Core;
using Utility.Interaction;
using Utility.Utils.Json;
using Utility.Utils.Property;

namespace Utility.Dialogue
{
    [Serializable]
    public class DialogueData
    {
        [FormerlySerializedAs("dialogues")] public DialogueElement[] dialogueElements;

        public int dialogueIdx;

        public UnityAction<int> ChooseAction;
        public UnityAction DialogueEndAction;

        public void Init(string json, float dialoguePrintSec = 0f, float nextSec = 0f)
        {
            dialogueElements = JsontoString.FromJsonArray<DialogueElement>(json);
            if (Application.isPlaying && Mathf.Approximately(dialoguePrintSec, 0))
            {
                dialoguePrintSec = DataController.Instance.dialoguePrintSec;
            }

            foreach (var dialogue in dialogueElements)
            {
                dialogue.printSec = dialoguePrintSec;
                dialogue.nextSec = nextSec;
            }

            dialogueIdx = 0;
        }
        
        public void Init(DialogueElement dialogueElement, float dialoguePrintSec = 0f, float nextSec = 0f)
        {
            dialogueElements = new[] { dialogueElement };
            if (Application.isPlaying && Mathf.Approximately(dialoguePrintSec, 0))
            {
                dialoguePrintSec = DataController.Instance.dialoguePrintSec;
            }

            foreach (var dialogue in dialogueElements)
            {
                dialogue.printSec = dialoguePrintSec;
                dialogue.nextSec = nextSec;
            }

            dialogueIdx = 0;
        }

        public void EndDialogue()
        {
            Debug.Log("초기화");
            dialogueElements = Array.Empty<DialogueElement>();
            dialogueIdx = 0;

            ChooseAction = null;

            var tAction = DialogueEndAction;
            DialogueEndAction = () => { };
            tAction?.Invoke();
        }
    }

    [Serializable]
    public class TaskData
    {
        public List<Task> tasks;

        public int taskIndex;

        //현재 순서 - 가독성을 위해 1부터 시작
        public int taskOrder = 1;
    }

    [Serializable]
    public class Task
    {
        public string name;
        public TaskContentType taskContentType;
        public string nextFile;
        public int order;
        public string condition;
        public string increaseVar;
    }

    public enum Expression
    {
        None = -1,
        Idle = 0,
        Laugh,
        Sad,
        Cry,
        Angry,
        Surprise,
        Panic,
        Suspicion,
        Fear,
        Curious
    }
    
    [Serializable]
    public class DialogueElement
    {
        public string name;
        public Expression expression;
        public string anim_name;
        public string contents;
        public string sfx;

        [FormerlySerializedAs("PrintSec")] [Space(5)]
        public float printSec;

        [FormerlySerializedAs("NextSec")] public float nextSec;


        [NonSerialized] public double startTime;
        [NonSerialized] public double endTime;

        [Header("카메라 뷰")] public bool isViewChange;
        [ConditionalHideInInspector("isViewChange")]
        public bool isOriginalCamInfo;
        [ConditionalHideInInspector("isViewChange")]
        public bool isTrackTransform;
        [ConditionalHideInInspector("isViewChange")]
        public bool isTrackMainCharacter;
        [FormerlySerializedAs("camInfo")] [ConditionalHideInInspector("isViewChange")]
        public CamInfo camOffsetInfo;
        
        [ConditionalHideInInspector("isTrackTransform")]
        public Transform trackTransform;
    }
}
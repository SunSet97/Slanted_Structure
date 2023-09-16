using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Utility.Core;
using Utility.Json;
using Utility.Property;

namespace Data
{
    [Serializable]
    public class DialogueData
    {
        public Dialogue[] dialogues;

        public int dialogueIdx;

        public UnityAction<int> ChooseAction;
        public UnityAction DialogueEndAction;

        public DialogueData()
        {
            Debug.Log("초기화");
            dialogues = Array.Empty<Dialogue>();
            dialogueIdx = 0;

            ChooseAction = null;
            DialogueEndAction = null;
        }

        public void Init(string json, float dialoguePrintSec = 0f, float nextSec = 0f)
        {
            Debug.Log("초기화");
            dialogues = JsontoString.FromJsonArray<Dialogue>(json);
            if (Application.isPlaying && Mathf.Approximately(dialoguePrintSec, 0))
            {
                dialoguePrintSec = DataController.Instance.dialoguePrintSec;
            }

            foreach (var dialogue in dialogues)
            {
                dialogue.printSec = dialoguePrintSec;
                dialogue.nextSec = nextSec;
            }

            dialogueIdx = 0;
        }

        public void Reset()
        {
            DialogueEndAction?.Invoke();
            Debug.Log("초기화");
            dialogues = Array.Empty<Dialogue>();
            dialogueIdx = 0;

            ChooseAction = null;
            DialogueEndAction = null;
        }
    }

    [Serializable]
    public class TaskData
    {
        public Task[] tasks;

        public int taskIndex;

        //현재 순서 - 가독성을 위해 1부터 시작
        public int taskOrder = 1;

        public bool isContinue = true;
    }

    [Serializable]
    public class Task
    {
        public string name;
        public CustomEnum.TaskContentType taskContentType;
        public string nextFile;
        public int order;
        public string condition;
        public string increaseVar;
    }

    [Serializable]
    public class Dialogue
    {
        public string name;
        public CustomEnum.Expression expression;
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
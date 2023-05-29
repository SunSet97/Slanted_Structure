using System;
using UnityEngine;
using UnityEngine.Events;
using Utility.Core;
using Utility.Json;

namespace Data
{
    [Serializable]
    public class DialogueData
    { 
        public Dialogue[] dialogues;
        
        public int dialogueIdx;

        [NonSerialized] public float DialoguePrintSec;
        [NonSerialized] public float NextSec;

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
            Debug.Log("추가");
            if (Mathf.Approximately(dialoguePrintSec, 0))
            {
                dialoguePrintSec = DataController.Instance.dialoguePrintSec;
            }
            foreach (var dialogue in dialogues)
            {
                dialogue.PrintSec = dialoguePrintSec;
                dialogue.NextSec = nextSec;
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
    public class Dialogue {
        public string name;
        public CustomEnum.Expression expression;
        public string anim_name;
        public string contents;
        public string sfx;

        [Space(5)]
        [NonSerialized] public float PrintSec;
        [NonSerialized] public float NextSec;
        
        [NonSerialized] public float startTime;
        [NonSerialized] public float endTime;
    }
}
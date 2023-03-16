using System;
using UnityEngine;
using UnityEngine.Events;
using Utility.Json;

namespace Data
{
    [Serializable]
    public class DialogueData
    { 
        public int dialogueIdx;
        
        public Dialogue[] dialogues;
        
        public UnityAction<int> ChooseAction;
        public UnityAction DialogueEndAction;
        
        public DialogueData()
        {
            dialogues = Array.Empty<Dialogue>();
            dialogueIdx = 0;

            ChooseAction = null;
            DialogueEndAction = null;
        }

        public void Init(string json)
        {
            dialogues = JsontoString.FromJsonArray<Dialogue>(json);
            dialogueIdx = 0;
        }

        public void Reset()
        {
            DialogueEndAction?.Invoke();
            
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
    }
}
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
        
        [NonSerialized]
        public CamInfo CamInfo;
        
        public UnityAction<int> ChooseAction;
        public UnityAction DialogueEndAction;
        
        public DialogueData()
        {
            dialogues = Array.Empty<Dialogue>();
            dialogueIdx = 0;
            CamInfo = new CamInfo();

            ChooseAction = null;
            DialogueEndAction = null;
        }

        public void Init(string json)
        {
            dialogues = JsontoString.FromJsonArray<Dialogue>(json);
            
            int peekIdx = dialogues.Length - 1;
            if (dialogues[peekIdx].name == "camera")
            {
                int[] camPos = Array.ConvertAll(dialogues[peekIdx].anim_name.Split(','), int.Parse);
                int[] camRot = Array.ConvertAll(dialogues[peekIdx].contents.Split(','), int.Parse);
                CamInfo.camDis = new Vector3(camPos[0], camPos[1], camPos[2]);
                CamInfo.camRot = new Vector3(camRot[0], camRot[1], camRot[2]);
            }
            dialogues = Array.FindAll(dialogues, item =>
                item.name != "camera"
            );
            
            dialogueIdx = 0;
        }

        public void Reset()
        {
            DialogueEndAction?.Invoke();
            
            dialogues = Array.Empty<Dialogue>();
            dialogueIdx = 0;
            CamInfo = new CamInfo();
            
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
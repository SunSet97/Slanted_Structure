using System;
using UnityEngine;
using Utility.Serialize;

namespace Data
{
    [Serializable]
    public class DialogueData
    {
        public int dialogueIdx;
        
        public Dialogue[] dialogues;
        
        [NonSerialized]
        public CamInfo CamInfo;
        
        public SerializableVector3 serializableCamInfo;
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
    }
}
using System;

namespace Data
{
    [Serializable]
    public class DialogueData
    {
        public Dialogue[] dialogues;
    }

    [Serializable]
    public class TaskData
    {

        public Task[] tasks;

        //현재 배열 인덱스
        public int taskIndex = 0;

        //현재 순서 - 가독성을 위해 1부터 시작
        public int taskOrder = 1;

        public bool isContinue = true;


    }

// 직렬화 클래스들
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
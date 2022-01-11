using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

public enum TYPE
{
    NONE = 0, ANIMATION, DIALOGUE, TEMP, TEMPEND, TaskReset, NEW, THEEND, TempDialogueEnd
}

public enum EXPRESSION
{
    IDLE = 0, LAUGH, SAD, CRY, ANGRY, SURPISE, PANIC, SUSPICION, FEAR, CURIOUS, ANIM_ONE, ANIM_TWO
}

// 직렬화 클래스들
[System.Serializable]
public class Task
{
    public string name;
    public TYPE type;
    public string nextFile;
    public int order;
    public string condition;
    public string increaseVar;
}

[System.Serializable]
public class Dialogue {
    public string name;
    public EXPRESSION experssion;
    public string anim_name;
    public string contents;
}

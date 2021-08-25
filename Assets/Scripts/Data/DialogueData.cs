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

    //현재 인덱스
    public int taskIndex = 0;

    //현재 순서 - 가독성을 위해
    public int taskOrder = 1;

    public bool isContinue = true;


}

public enum TYPE
{
    NONE, ANIMATION, DIALOGUE, TEMP, TEMPEND, TASKEND, NEW, THEEND
}

public enum EXPRESSION
{
    IDLE, LAUGH, SAD, CRY, ANGRY, SURPISE, PANIC, SUSPICION, FEAR, CURIOUS
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
    public string kind;
    public string contents;
}
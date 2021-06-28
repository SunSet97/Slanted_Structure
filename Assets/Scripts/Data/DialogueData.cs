using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class DialogueData
{
    // 대사를 담은 배열
    public Dialogue[] dialogue;

    // 선택지를 담은 배열
    public Choice[] choice;

    // 특정 선택지를 선택했을 때 씬 변환 여부 확인(bool)
    public IsSceneChange[] isSceneChange;

    // 이동할 씬 이름 저장 배열(string)
    // 이동할 맵으로 바뀔 가능성 있음 (추가가능성 있음) 
    public NextScene[] nextScene;

    // 특정 선택지를 선택했을 때의 대화 진행 여부
    public IsContinue[] isContinue;

    // 스토리 큰 단위 매개변수, 1부터시작
    public StoryParam[] storyParam;

    // 스토리 분기 선택지 매개변수
    public BranchParam[] branchParam;

    // 두번째 스토리 분기 선택지 매개변수 
    public ScndBranchParam[] scndBranchParam;

    // 선택지에 따른 친밀도 변경값 
    public IntimacyParam[] intimacyParam;

    // 선택지에 따른 자존감 증감값 -> DialogueController에서 사용
    public SelfEstmParam[] selfEstmParam;

    // 선택지가 나오는 조건1 -> 캐릭터 간의 친밀도: 아래의 경계값확인
    public IntimacyCrt[] intimacyCrt;

    // 선택지가 나오는 조건2 -> 캐릭터의 자존감: 아래의 경계값확인
    public SelfEstmCrt[] selfEstmCrt;

    // 대화 인덱스 매개변수
    public DialogueIndexParam[] dialogueIndexParam;

    // 몇 번째 대화로 갈 지 정하는 변수 
    public NextDialogueParam[] nextDialogueParam; 
}

// 직렬화 클래스들 

[Serializable]
public class Dialogue {
    public string[] dialogueScript;
}

[Serializable]
public class Choice
{
    public string[] choiceOption;
}

[Serializable]
public class IsSceneChange
{
    public bool[] sceneChange;
}

[Serializable]
public class NextScene
{
    public string[] sceneName;
}

[Serializable]
public class IsContinue
{
    public bool[] dialogueContinue;
}

[Serializable]
public class StoryParam
{
    public int[] storyNum;
}

[Serializable]
public class BranchParam
{
    public int[] branchNum;
}

[Serializable]
public class ScndBranchParam
{
    public int[] scndBranchNum;
}

[Serializable]
public class IntimacyParam
{
    public int[] spOun;
    public int[] spRau;
    public int[] ounRau;
}

[Serializable]
public class SelfEstmParam
{
    public int[] selfEstm;
}

[Serializable]
public class IntimacyCrt
{
    public int[] spOun;
    public int[] spRau;
    public int[] ounRau;
}

[Serializable]
public class SelfEstmCrt
{
    public int[] selfEstm;
}

[Serializable]
public class DialogueIndexParam
{
    public int[] index;
}

[Serializable]
public class NextDialogueParam
{
    public int[] dialogueNum;
}
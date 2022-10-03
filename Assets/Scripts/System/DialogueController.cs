﻿using System;
using Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Data.CustomEnum;

public class DialogueController : MonoBehaviour
{
    private static DialogueController _instance;
    
    public static DialogueController instance
    {
        get { return _instance; }
    }
    
    public GameObject dialoguePanel;

    public Animator charEmotionAnimator;

    public Text dialogueNameText;
    public Text dialogueContentText;

    public Transform choiceRoot;
    private GameObject[] choiceBtns;
    private Text[] choiceTexts;


    private int dialogueIdx;

    internal bool isTalking;
    
    internal Vector3 dialogueCameraPos;
    internal Vector3 dialogueCameraRot;
    
    private UnityAction<int> chooseAction;
    private UnityAction dialoguePrevAction;
    private UnityAction dialogueEndAction;

    private static readonly int Emotion = Animator.StringToHash("Emotion");
    
    void Awake()
    {
        _instance = this;
    }
    void Start()
    {
        var childCount = choiceRoot.childCount;
        choiceBtns = new GameObject[childCount];
        choiceTexts = new Text[childCount];
        for (int i = 0; i < childCount; i++)
        {
            choiceBtns[i] = choiceRoot.GetChild(i).gameObject;
            choiceTexts[i] = choiceBtns[i].GetComponentInChildren<Text>(true);
        }

        EventTrigger eventTrigger = dialoguePanel.GetComponent<EventTrigger>();
        EventTrigger.Entry entryPointerUp = new EventTrigger.Entry();
        entryPointerUp.eventID = EventTriggerType.PointerUp;
        entryPointerUp.callback.AddListener(data => { UpdateWord(); });
        eventTrigger.triggers.Add(entryPointerUp);
    }

    public void Initialize()
    {
        dialoguePrevAction = null;
        dialogueEndAction = null;
        dialoguePanel.SetActive(false);
    }

    public string ConvertPathToJson(string path)
    {
        Debug.Log("변환 전: " + path);
        string dialogueName = path.Split('/')[1];
        Debug.Log("변환 후: " + dialogueName);
        return DataController.instance.dialogueDB.LoadAsset<TextAsset>(dialogueName).text;
    }

    public void StartConversation(string jsonString)
    {
        if (DataController.instance.taskData != null)
            DataController.instance.taskData.isContinue = false;

        Debug.Log("1");
        isTalking = true;
        //Joystick 중지
        DataController.instance.StopSaveLoadJoyStick(true);
        Debug.Log("2");
        DataController.instance.dialogueData.dialogues = JsontoString.FromJsonArray<Dialogue>(jsonString);
        dialogueIdx = 0;
        Debug.Log("3");
        if (dialoguePrevAction != null)
        {
            dialoguePrevAction();
            dialoguePrevAction = null;
        }
        Debug.Log("4");
        Debug.Log(dialoguePanel.activeSelf);
        dialoguePanel.SetActive(true);
        Debug.Log(dialoguePanel.activeSelf);
        Debug.Log("5");
        // 시작할 때 
        DialogueData dialogueData = DataController.instance.dialogueData;
        int peekIdx = dialogueData.dialogues.Length - 1;
        if (dialogueData.dialogues[peekIdx].name == "camera")
        {
            Debug.Log("111");
            int[] camPos = Array.ConvertAll(dialogueData.dialogues[peekIdx].anim_name.Split(','), int.Parse);
            int[] camRot = Array.ConvertAll(dialogueData.dialogues[peekIdx].contents.Split(','), int.Parse);
            Debug.Log("222");
            dialogueCameraPos = new Vector3(camPos[0], camPos[1], camPos[2]);
            dialogueCameraRot = new Vector3(camRot[0], camRot[1], camRot[2]);
            Debug.Log("333");
        }
        Debug.Log("6");

        dialogueData.dialogues = Array.FindAll(dialogueData.dialogues, item =>
            item.name != "camera"
        );
        UpdateWord();
    }

    public void UpdateWord()
    {
        DialogueData dialogueData = DataController.instance.dialogueData;
        int dialogueLen = dialogueData.dialogues.Length;
        // 대화가 끝나면 선택지 부를 지 여부결정
        if (dialogueIdx >= dialogueLen)
        {
            dialogueCameraPos = Vector3.zero;
            dialogueCameraRot = Vector3.zero;
            isTalking = true;
            dialoguePanel.SetActive(false);
            DataController.instance.StopSaveLoadJoyStick(false);
            foreach (var positionSet in DataController.instance.currentMap.positionSets)
            {
                DataController.instance.GetCharacter(positionSet.who).Emotion = Expression.IDLE;   
            }
            foreach (var positionSet in DataController.instance.currentMap.characters)
            {
                // if (Character.TryParse(dialogueData.dialogues[dialogueIdx].anim_name, out Character who))
                DataController.instance.GetCharacter(positionSet.who).Emotion = Expression.IDLE;   
            }
            
            dialogueIdx = 0;
            if (dialogueEndAction != null)
            {
                dialogueEndAction();
                dialogueEndAction = null;
            }
            dialogueData.dialogues = null;
            if (DataController.instance.taskData != null)
            {
                DataController.instance.taskData.isContinue = true;
            }
        }
        else
        {
            // 캐릭터 표정 업데이트 (애니메이션)
            if (!string.IsNullOrEmpty(dialogueData.dialogues[dialogueIdx].anim_name))
            {
                if (charEmotionAnimator)
                {
                    Debug.Log(dialogueData.dialogues[dialogueIdx].anim_name);
                    string path = "Character_dialogue/" + dialogueData.dialogues[dialogueIdx].anim_name;
                    // charDialogueAnimator.runtimeAnimatorController =
                    //     Resources.Load(path) as UnityEditor.Animations.AnimatorController;
                    charEmotionAnimator.runtimeAnimatorController =
                        Resources.Load(path) as RuntimeAnimatorController;
                    if (charEmotionAnimator.runtimeAnimatorController != null)
                    {
                        charEmotionAnimator.GetComponent<Image>().enabled = true;
                        Debug.Log((int) dialogueData.dialogues[dialogueIdx].expression + "  " +
                                  dialogueData.dialogues[dialogueIdx].expression);
                        charEmotionAnimator.SetInteger(Emotion, ((int) dialogueData.dialogues[dialogueIdx].expression));

                        if (Character.TryParse(dialogueData.dialogues[dialogueIdx].anim_name, out Character who))
                        {
                            DataController.instance.GetCharacter(who).Emotion = dialogueData.dialogues[dialogueIdx].expression;   
                        }
                    }
                    else
                    {
                        charEmotionAnimator.GetComponent<Image>().enabled = false;
                        // Debug.LogError("Dialogue 애니메이션 세팅 오류");
                    }
                }
                else
                {
                    Debug.LogError("Canvas에 캐릭터 표정 charAnimator 넣으세요");
                }
            }
            else
            {
                charEmotionAnimator.runtimeAnimatorController = null;
                charEmotionAnimator.GetComponent<Image>().enabled = false;
            }

            // 대화 텍스트 업데이트

            // 현재 실행 중인 캐릭터에 대해서
            // 현재 맵에 기준???    햄버거 집 스핏의 경우 임시로 존재하는 거니까
            // MapData에 inspector window에서 setting, position setting되어있는 캐릭터들도 추가
            // 대화 시 무슨 캐릭터인지 anim_name으로 Find (who를 사용)
            MapData.AnimationCharacterSet animator = DataController.instance.currentMap.characters.Find(
                item => item.who.ToString().Equals(dialogueData.dialogues[dialogueIdx].anim_name));
            // DataController.instance.GetCharacter(animator.who).emotion =(int) dialogueData.dialogues[dialogueIdx].expression 
            animator?.characterAnimator.SetInteger(Emotion, (int)dialogueData.dialogues[dialogueIdx].expression);
            
            dialogueNameText.text = dialogueData.dialogues[dialogueIdx].name; // 이야기하는 캐릭터 이름
            dialogueContentText.text = dialogueData.dialogues[dialogueIdx].contents; // 캐릭터의 대사
            dialogueIdx++;
        }
    }
    
    public void OpenChoicePanel()
    {
        DataController.instance.StopSaveLoadJoyStick(true);
        TaskData currentTaskData = DataController.instance.taskData;
        int index = currentTaskData.taskIndex;
        int choiceLen = int.Parse(currentTaskData.tasks[index].nextFile);

        int[] likeable = DataController.instance.GetLikeable();

        choiceBtns[0].transform.parent.gameObject.SetActive(true);
        if (currentTaskData.tasks.Length <= index + choiceLen)
        {
            Debug.LogError("선택지 개수 오류 - IndexOverFlow");
        }
        // 선택지 개수와 조건에 맞게 선택지가 나올 수 있도록 함.
        for (int i = 0; i < choiceBtns.Length; i++)
        {
            if (choiceLen <= i)
            {
                choiceBtns[i].SetActive(false);
                continue;
            }
            // 친밀도와 자존감이 기준보다 낮으면 일부 선택지가 나오지 않을 수 있음
            var choiceIndex = index + i + 1;
            var choiceTask = currentTaskData.tasks[choiceIndex];
            
            choiceTask.condition = choiceTask.condition.Replace("m", "-");
            Debug.Log($"{i}번째 선택지 조건 - {choiceTask.condition}");
            int[] condition = Array.ConvertAll(choiceTask.condition.Split(','), int.Parse);

            //양수인 경우 이상, 음수인 경우 이하
            if (choiceTask.order >= 0)
            {
                if (condition[0] >= likeable[0] && condition[1] >= likeable[1] && condition[2] >= likeable[2])
                {
                    choiceBtns[i].SetActive(true);
                    choiceTexts[i].text = choiceTask.name;
                }    
            }
            else
            {
                if (condition[0] <= likeable[0] && condition[1] <= likeable[1] && condition[2] <= likeable[2])
                {
                    choiceBtns[i].SetActive(true);
                    choiceTexts[i].text = choiceTask.name;
                }    
            }
        }
    }
    
    public void SetChoiceAction(UnityAction<int> choiceAction)
    {
        chooseAction += choiceAction;
    }
    public void SetDialouguePrevAction(UnityAction prevAction)
    {
        dialoguePrevAction += prevAction;
    }
    public void SetDialougueEndAction(UnityAction endAction)
    {
        dialogueEndAction += endAction;
    }
    /// <summary>
    /// 대화 선택지를 삭제하는 함수
    /// </summary>
    void RemoveChoice()
    {
        choiceBtns[0].transform.parent.gameObject.SetActive(false);
    }

    // 선택지를 눌렀을 때 불리는 함수, Index 1부터 시작
    public void PressChoice(int index)
    {
        RemoveChoice();
        DataController.instance.StopSaveLoadJoyStick(false);
        chooseAction(index);
        
        // currentTaskData.taskIndex += choiceLen + 1;
    }
}

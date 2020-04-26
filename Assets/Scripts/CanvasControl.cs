﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using UnityEngine.SceneManagement;

public class CanvasControl : MonoBehaviour
{
    [Header("저장 패널")]
    public Text[] saveText = new Text[3];
    public GameObject savePanel;

    // 추후 변경 
    [Header("친밀도/자존감")]
    public Text intimacyText_speat;
    public Text intimacyText_oun;
    public Text selfEstmText;

    [Header("대화 관련")]

    public GameObject DialoguePanel;
    public bool isPossibleCnvs = true;
    public GameObject[] choiceBtn = new GameObject[4];
    public Text[] choice = new Text[4];
    public Text speakerName;
    public Text speakerWord;

    // 대화 관련 변수 
    private int cnvsCnt = 0; // 대화 카운트 변수 
    private int dialogueCnt = 0; // 대사 카운트 변수
    private int dialogueLen = 0; // 대사의 갯수
    private int choiceLen = 0; // 선택지 갯수

    private bool isExistFile;

    private void Start()
    {
        if (DataController.instance_DataController != null && selfEstmText != null)
        {
            selfEstmText.text = "자존감: " + DataController.instance_DataController.charData.selfEstm;
            intimacyText_speat.text = "스핏 친밀도: " + DataController.instance_DataController.charData.intimacy_spOun;
            intimacyText_oun.text = "오운 친밀도: " + DataController.instance_DataController.charData.intimacy_ounRau;
        }

    }

    // 씬 로드 시 사용 
    // 새 게임 시작할 때의 씬이름 나중에 넣을 것!! 
    public void OpenScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);

        // 새 게임 시작할 경우 새 데이터 파일 생성하도록
        // 씬 이름이 정해지면 주석 풀 예정

        //if (sceneName == "newGame")
        //{
        //    DataController.Instance.LoadCharData("NewData.json");
        //}

        DataController.instance_DataController.LoadData("Save", "NewData.json");

        // 씬 이동 시 현재 씬 이름 데이터 파일에 저장
        DataController.instance_DataController.charData.currentScene = sceneName;
    }

    //세이브 된 게임 데이터 파일을 여는 함수
    public void SaveFileOpen(int fileNum)
    {
        if (DataController.instance_DataController.isExistdata[fileNum])
        {
            DataController.instance_DataController.LoadData("Save", "SaveData" + fileNum);

            SceneManager.LoadScene(DataController.instance_DataController.charData.currentScene);
            print("SaveData" + fileNum + ".json");

        }

    }

    // 패널 열 때 사용 
    public void OpenPanel(string panelName)
    {
        if (panelName == "SavePanel" || panelName == "SelectData")
        {
            savePanel.SetActive(true);
            // 세이브 데이터 패널이 열릴 때마다 각 세이브 파일이 존재하는 지 확인 
            DataController.instance_DataController.ExistsData();

            for (int i = 0; i < 3; i++)
            {

                if (DataController.instance_DataController.isExistdata[i])
                {
                    // 해당 칸에 데이턱 존재하면 버튼 텍스트 업데이트
                    saveText[i].text = "FULL DATA";

                }
                else
                {

                    // 데이터 없을 때
                    saveText[i].text = "NO DATA";
                }
            }
        }
    }

    public void ClosePanel(string panelName)
    {
        if (panelName == "SavePanel")
        {
            savePanel.SetActive(false);
        }

    }

    // 자존감 및 친밀도 text 업데이트
    public void UpdateStats(){

        selfEstmText.text = "자존감: " + DataController.instance_DataController.charData.selfEstm;
        intimacyText_speat.text = "스핏 친밀도: " + DataController.instance_DataController.charData.intimacy_spOun;
        intimacyText_oun.text = "오운 친밀도: " + DataController.instance_DataController.charData.intimacy_ounRau;

    }

    // 대화 관련 함수

    // npc를 터치하면 불리는 함수
    public void StartConversation()
    {
        dialogueLen = DataController.instance_DataController.dialogueData.dialogue[cnvsCnt].dialogueScript.Length;
        dialogueCnt = 0;
        DialoguePanel.SetActive(true);
        UpdateWord();
    }

    public void UpdateWord()
    {
        // 대화가 끝나면 선택지 부를 지 여부결정 
        if (dialogueCnt >= dialogueLen)
        {
            DialoguePanel.SetActive(false);
            print(DataController.instance_DataController.dialogueData.choice[cnvsCnt].choiceOption.Length);

            // 부를 선택지 정보가 있는가 없는 가 
            if (DataController.instance_DataController.dialogueData.choice[cnvsCnt].choiceOption.Length > 0)
            {
                OpenChoicePanel();
            }
            else
            {
                // 대화가 끝났으니 다시 대화 가능하도록 
                isPossibleCnvs = true;
            }
        }
        else
        {
            // 대화가 진행되는 중 텍스트 업데이트
            speakerName.text = DataController.instance_DataController.dialogueData.dialogue[cnvsCnt].dialogueScript[dialogueCnt]; // 이야기하는 캐릭터 이름
            speakerWord.text = DataController.instance_DataController.dialogueData.dialogue[cnvsCnt].dialogueScript[dialogueCnt + 1]; // 캐릭터의 대사
            dialogueCnt += 2;
        }

    }

    // 선택지가 있을 때 선택지 패널 염 
    void OpenChoicePanel()
    {
        choiceLen = DataController.instance_DataController.dialogueData.choice[cnvsCnt].choiceOption.Length;

        int curIntimacy_spOun = DataController.instance_DataController.charData.intimacy_spOun;
        int curIntimacy_spRau = DataController.instance_DataController.charData.intimacy_spRau;
        int curIntimacy_ounRau = DataController.instance_DataController.charData.intimacy_ounRau;

        // 라우가 플레이어일 때만 확인하나 ???? 
        //if (DataController.instance_DataController.charData.curPlayer == "Rau")
        //{
        int curSelfEstm = DataController.instance_DataController.charData.selfEstm;


        // 선택지 개수와 조건에 맞게 선택지가 나올 수 있도록 함.
        for (int i = 0; i < choiceLen; i++)
        {
            // 친밀도와 자존감이 기준보다 낮으면 일부 선택지가 나오지 않을 수 있음 

            if (
               curIntimacy_spRau >= DataController.instance_DataController.dialogueData.intimacyCrt[cnvsCnt].spRau[i] &&
               curIntimacy_spOun >= DataController.instance_DataController.dialogueData.intimacyCrt[cnvsCnt].spOun[i] &&
               curIntimacy_ounRau >= DataController.instance_DataController.dialogueData.intimacyCrt[cnvsCnt].ounRau[i] &&
               curSelfEstm >= DataController.instance_DataController.dialogueData.selfEstmCrt[cnvsCnt].selfEstm[i]
              )
            {
                choiceBtn[i].SetActive(true);
                choice[i].text = DataController.instance_DataController.dialogueData.choice[cnvsCnt].choiceOption[i];
            }
        }
    }

    // 선택지를 눌렀을 때 불리는 함수 
    public void PressChoice(int i)
    {
        for (int j = 0; j < choiceLen; j++)
        {
            // 선택지 버튼 없앰
            choiceBtn[j].SetActive(false);

        }

        // 씬 바꿀 일 있을 때 씬 변경하는 함수 부름
        if (DataController.instance_DataController.dialogueData.isSceneChange[cnvsCnt].sceneChange[i])
        {
            OpenScene(DataController.instance_DataController.dialogueData.nextScene[cnvsCnt].sceneName[i]);
        }


        // 선택에 따른 스토리 업데이트
        if (DataController.instance_DataController.dialogueData.storyParam[cnvsCnt].storyNum[i] != 0)
            DataController.instance_DataController.charData.story = DataController.instance_DataController.dialogueData.storyParam[cnvsCnt].storyNum[i];

        // 선택에 따른 스토리 분기 업데이트
        if (DataController.instance_DataController.dialogueData.branchParam[cnvsCnt].branchNum[i] != 0)
            DataController.instance_DataController.charData.storyBranch = DataController.instance_DataController.dialogueData.branchParam[cnvsCnt].branchNum[i];

        if (DataController.instance_DataController.dialogueData.scndBranchParam[cnvsCnt].scndBranchNum[i] != 0)
            DataController.instance_DataController.charData.storyBranch_scnd = DataController.instance_DataController.dialogueData.scndBranchParam[cnvsCnt].scndBranchNum[i];


        // 선택에 따른 친밀도, 자존감 업데이트
        DataController.instance_DataController.charData.intimacy_spRau += DataController.instance_DataController.dialogueData.intimacyParam[cnvsCnt].spRau[i];
        DataController.instance_DataController.charData.intimacy_spOun += DataController.instance_DataController.dialogueData.intimacyParam[cnvsCnt].spOun[i];
        DataController.instance_DataController.charData.intimacy_ounRau += DataController.instance_DataController.dialogueData.intimacyParam[cnvsCnt].ounRau[i];

        DataController.instance_DataController.charData.selfEstm += DataController.instance_DataController.dialogueData.selfEstmParam[cnvsCnt].selfEstm[i];

        // 자존감, 친밀도 텍스트 업데이트
        UpdateStats();

        // 대화가 계속 되는 지 
        if (DataController.instance_DataController.dialogueData.isContinue[cnvsCnt].dialogueContinue[i])
        {
            cnvsCnt = DataController.instance_DataController.dialogueData.nextDialogueParam[cnvsCnt].dialogueNum[i];
            StartConversation();
        }
        else
        {
            // 이어지는 대화가 없을 때 
            // 다시 대화 가능하도록 (처음부터 대화 하는 것을 말함) 
            cnvsCnt = 0;
            isPossibleCnvs = true;
        }

    }
}

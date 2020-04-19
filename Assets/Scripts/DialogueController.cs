using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    public GameObject DialoguePanel;
    public CanvasControl canvasCtrl;
    public bool isPossibleCnvs = true;
    public GameObject[] choiceBtn = new GameObject[4];
    public Text[] choice = new Text[4];

    public Text speakerName;
    public Text speakerWord;

    int cnvsCnt = 0; // 대화 카운트 변수 
    int dialogueCnt = 0; // 대사 카운트 변수
    int dialogueLen = 0; // 대사의 갯수
    int choiceLen = 0; // 선택지 갯수

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

            if(
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
            GameObject.Find("SceneController").GetComponent<SceneController>().OpenScene(DataController.instance_DataController.dialogueData.nextScene[cnvsCnt].sceneName[i]);
        }


        // 선택에 따른 스토리 업데이트
        if (DataController.instance_DataController.dialogueData.storyParam[cnvsCnt].storyNum[i] != 0)
            DataController.instance_DataController.charData.story = DataController.instance_DataController.dialogueData.storyParam[cnvsCnt].storyNum[i];

        // 선택에 따른 스토리 분기 업데이트
        if(DataController.instance_DataController.dialogueData.branchParam[cnvsCnt].branchNum[i] != 0)
            DataController.instance_DataController.charData.storyBranch = DataController.instance_DataController.dialogueData.branchParam[cnvsCnt].branchNum[i];

        if (DataController.instance_DataController.dialogueData.scndBranchParam[cnvsCnt].scndBranchNum[i] != 0)
            DataController.instance_DataController.charData.storyBranch_scnd = DataController.instance_DataController.dialogueData.scndBranchParam[cnvsCnt].scndBranchNum[i];
        

        // 선택에 따른 친밀도, 자존감 업데이트
        DataController.instance_DataController.charData.intimacy_spRau += DataController.instance_DataController.dialogueData.intimacyParam[cnvsCnt].spRau[i];
        DataController.instance_DataController.charData.intimacy_spOun += DataController.instance_DataController.dialogueData.intimacyParam[cnvsCnt].spOun[i];
        DataController.instance_DataController.charData.intimacy_ounRau += DataController.instance_DataController.dialogueData.intimacyParam[cnvsCnt].ounRau[i];
        
        DataController.instance_DataController.charData.selfEstm += DataController.instance_DataController.dialogueData.selfEstmParam[cnvsCnt].selfEstm[i];

        // 자존감, 친밀도 텍스트 업데이트
        canvasCtrl.UpdateStats();

        // 대화가 계속 되는 지 
        if (DataController.instance_DataController.dialogueData.isContinue[cnvsCnt].dialogueContinue[i])
        {
            cnvsCnt = DataController.instance_DataController.dialogueData.nextDialogueParam[cnvsCnt].dialogueNum[i];
            StartConversation();
        } else
        {
            // 이어지는 대화가 없을 때 
            // 다시 대화 가능하도록 (처음부터 대화 하는 것을 말함) 
            cnvsCnt = 0;
            isPossibleCnvs = true;
        }
        
    }
}
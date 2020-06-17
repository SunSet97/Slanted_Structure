using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    public GameObject DialoguePanel;
    public CanvasControl canvasCtrl;
    public bool isPossibleCnvs = true;
    public GameObject[] choiceBtn;
    public Text[] choice;

    public Text speakerName;
    public Text speakerWord;
    

    int cnvsCnt = 0;
    int cnvsLen = 0;
    int choiceLen = 0;

    // npc를 터치하면 불리는 함수
    public void StartConversation()
    {
        cnvsLen = DataController.instance_DataController.dialogueData.dialogue.Length;
        cnvsCnt = 0;
        DialoguePanel.SetActive(true);
        UpdateWord();
    }

    public void UpdateWord()
    {
        // 대화가 끝나면 선택지 부를 지 여부결정 
        if (cnvsCnt >= cnvsLen)
        {
            DialoguePanel.SetActive(false);

            // 부를 선택지 정보가 있는가 없는 가 
            if (DataController.instance_DataController.dialogueData.choice.Length > 0)
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
            speakerName.text = DataController.instance_DataController.dialogueData.dialogue[cnvsCnt]; // 이야기하는 캐릭터 이름
            speakerWord.text = DataController.instance_DataController.dialogueData.dialogue[cnvsCnt + 1]; // 캐릭터의 대사
            cnvsCnt += 2;
        }
        
    }

    // 선택지가 있을 때 선택지 패널 염 
    void OpenChoicePanel()
    {
        choiceLen = DataController.instance_DataController.dialogueData.choice.Length;

        int curIntimacy_speat = DataController.instance_DataController.charData.intimacy_speat;
        int curIntimacy_oun = DataController.instance_DataController.charData.intimacy_oun;
        int curSelfEstm = DataController.instance_DataController.charData.selfEstm;


        // 선택지 개수와 조건에 맞게 선택지가 나올 수 있도록 함.
        for (int i = 0; i < choiceLen; i++)
        {
            // 친밀도와 자존감이 기준보다 낮으면 일부 선택지가 나오지 않을 수 있음 
            if(/*curIntimacy_speat >= DataController.instance_DataController.dialogueData.intimacyCrt[i] */
                /*&&*/ curSelfEstm >= DataController.instance_DataController.dialogueData.selfEstmCrt[i])
            {
                choiceBtn[i].SetActive(true);
                choice[i].text = DataController.instance_DataController.dialogueData.choice[i];
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
        if (DataController.instance_DataController.dialogueData.isSceneChange[i])
        {
            GameObject.Find("SceneController").GetComponent<SceneController>().OpenScene(DataController.instance_DataController.dialogueData.nextScene[i]);
        }


        // 선택에 따른 스토리 업데이트
        if (DataController.instance_DataController.dialogueData.storyParam[i] != 0)
            DataController.instance_DataController.charData.story = DataController.instance_DataController.dialogueData.storyParam[i];

        // 선택에 따른 스토리 분기 업데이트
        if(DataController.instance_DataController.dialogueData.branchParam[i] != 0)
            DataController.instance_DataController.charData.story_branch = DataController.instance_DataController.dialogueData.branchParam[i];

        // 선택에 따른 친밀도, 자존감 업데이트
        DataController.instance_DataController.charData.intimacy_speat += DataController.instance_DataController.dialogueData.intimacyPram_speat[i];
        DataController.instance_DataController.charData.intimacy_oun += DataController.instance_DataController.dialogueData.intimacyPram_oun[i];
        DataController.instance_DataController.charData.selfEstm += DataController.instance_DataController.dialogueData.selfEstmPram[i];

        // 자존감, 친밀도 텍스트 업데이트
        canvasCtrl.UpdateStats();

        // 조건 추가: 이어지는 대화가 없을 때 
        // 다시 대화 가능하도록 
        isPossibleCnvs = true;
    }
}
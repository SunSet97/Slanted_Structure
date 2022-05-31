using System;
using System.Collections;
using Data;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using static Data.CustomEnum;

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
    public GameObject dialoguePanel;
    public bool isPossibleCnvs = true;
    public Transform choiceParent;
    private GameObject[] choiceBtn;
    private Text[] choiceText;
    public Text speakerName;
    public Text speakerWord;

    // 대화 관련 변수 
    public int dialogueCnt = 0; // 대사 카운트 변수
    private UnityAction<int> pressBtnMethod;
    private UnityAction endDialogueAction;
    private UnityAction startDialogueAction;

    public Animator charDialogueAnimator;

    private bool isExistFile;

    [Header("이야기 진행")]
    public int progressIndex = 1; // 이야기 진행을 돕는 콜라이더(ProgressCollider)의 인덱스 (맵 전환 시 1으로)
    public int commandIndex = 0; // 튜토리얼 지시문의 인덱스 (맵 전환 시 0으로)
    public bool isGoNextStep = false; // 다음 단계로 넘어갈 것인지 확인
    public Text commandText;
    public GameObject commandPanel;

    [Header("디버깅용")]
    public InputField mapcode; //맵코드
    public Toggle[] selectedCharacter; //선택된 캐릭터
    public GameObject selectedGuest;
    //인스턴스화
    private static CanvasControl _instance;
    public static CanvasControl instance
    {
        get => _instance;
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        if (choiceParent != null)
        {
            var childCount = choiceParent.childCount;
            choiceBtn = new GameObject[childCount];
            choiceText = new Text[childCount];
            for (int i = 0; i < childCount; i++)
            {
                choiceBtn[i] = choiceParent.GetChild(i).gameObject;
                choiceText[i] = choiceBtn[i].GetComponentInChildren<Text>(true);
            }
        }

        if (DataController.instance != null && selfEstmText != null)
        {
            selfEstmText.text = "자존감: " + DataController.instance.charData.selfEstm;
            intimacyText_speat.text = "스핏 친밀도: " + DataController.instance.charData.intimacy_spOun;
            intimacyText_oun.text = "오운 친밀도: " + DataController.instance.charData.intimacy_ounRau;
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

        DataController.instance.LoadData("Save", "NewData.json");

        // 씬 이동 시 현재 씬 이름 데이터 파일에 저장
        DataController.instance.charData.currentScene = sceneName;
    }

    //세이브 된 게임 데이터 파일을 여는 함수
    public void SaveFileOpen(int fileNum)
    {
        if (DataController.instance.isExistdata[fileNum])
        {
            DataController.instance.LoadData("Save", "SaveData" + fileNum);

            SceneManager.LoadScene(DataController.instance.charData.currentScene);
            print("SaveData" + fileNum + ".json");

        }

    }

    string objName;

    // 열고 닫을 패널이 어느 오브젝트 안에 있는 지 확인하는 함수
    public void GetObjName(int num)
    {
        // Minigame안의 패널인지, Mainstory안의 패널인지 구분하기 위해 사용 
        // 0: Mainstory 1:Minigame 2: StartScene에 있는 패널

        if (num == 0) objName = "Mainstory";
        else if (num == 1) objName = "Minigame";
        else objName = "Canvas";
    }

    // 패널 열 때 사용 
    public void OpenPanel(string panelName)
    {

        // 아래의 패널들은 패널을 열 때마다 정보를 업데이트 해야하므로 따로 관리
        if (panelName == "SavePanel" || panelName == "SelectData")
        {
            savePanel.SetActive(true);
            // 세이브 데이터 패널이 열릴 때마다 각 세이브 파일이 존재하는 지 확인 
            DataController.instance.ExistsData();

            for (int i = 0; i < 3; i++)
            {

                if (DataController.instance.isExistdata[i])
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
        //else
        //{
        //    if (objName == "Mainstory" || objName == "Minigame")
        //        transform.Find(objName).Find(panelName).gameObject.SetActive(true);
        //    else
        //        transform.Find(panelName).gameObject.SetActive(true);
        //}
    }

    // 데이터 저장 버튼을 누르면 불리는 함수
    public void SelectData(int fileNum)
    {
        //if (canvasCtrl == null)
        //{
        //    canvasCtrl = CanvasControl.instance_CanvasControl;
        //}


        if (DataController.instance.charData.pencilCnt > 0)
        {
            // 기존 데이터 없을 경우 버튼 텍스트 업데이트
            if (DataController.instance.isExistdata[fileNum] == false)
            {
                DataController.instance.isExistdata[fileNum] = true;
                saveText[fileNum].text = "FULL DATA";
            }
            // 데이터 저장 시 연필 개수, 캐릭터 위치, 현재 씬 등 업데이트 (점점 추가할 예정)
            DataController.instance.charData.pencilCnt -= 1;
            DataController.instance.charData.currentCharPosition = DataController.instance.GetCharacter(Character.Main).transform.position;
            DataController.instance.charData.rauPosition = DataController.instance.GetCharacter(Character.Rau).transform.position;
            DataController.instance.charData.speatPosition = DataController.instance.GetCharacter(Character.Speat).transform.position;
            DataController.instance.charData.ounPosition = DataController.instance.GetCharacter(Character.Oun).transform.position;
            DataController.instance.charData.currentScene = SceneManager.GetActiveScene().name;

            DataController.instance.SaveCharData("SaveData" + fileNum);

        }
    }


    public void ClosePanel(string panelName)
    {
        // 인게임씬의 캔버스일 때와 스타트씬의 캔버스일 때를 분리
        if (objName == "Mainstory" || objName == "Minigame")
            transform.Find(objName).Find(panelName).gameObject.SetActive(false);
        else
            transform.Find(panelName).gameObject.SetActive(false);

    }

    // 자존감 및 친밀도 text 업데이트
    public void UpdateStats(){

        selfEstmText.text = "자존감: " + DataController.instance.charData.selfEstm;
        intimacyText_speat.text = "스핏 친밀도: " + DataController.instance.charData.intimacy_spOun;
        intimacyText_oun.text = "오운 친밀도: " + DataController.instance.charData.intimacy_ounRau;

    }

    bool isExistCmdSpr; // 지시문과 같이 나올 이미지 존재 여부
    GameObject curCmdSpr; // 지시문과 같이 나올 이미지의 parent
    public SpriteRenderer[] sprRenderers; // curCmdSpr의 렌더러
    Coroutine fadeIn;
    public void TutorialCmdCtrl()
    {
        // 지시문과 함께 나올 이미지가 있을 때, 그 오브젝트를 배열로 받음 
        if (DataController.instance.commandSprite.Find(commandIndex.ToString()) != null)
        {
            isExistCmdSpr = true;
            curCmdSpr = DataController.instance.commandSprite.Find(commandIndex.ToString()).gameObject;
            sprRenderers = new SpriteRenderer[curCmdSpr.transform.childCount];
            sprRenderers = curCmdSpr.GetComponentsInChildren<SpriteRenderer>();
        }
        else
            isExistCmdSpr = false;

        // 페이드 인 (지시문 패널 보이게)
        commandPanel.SetActive(true);

        finishFadeIn = false;
        fadeIn = StartCoroutine(FadeIn());

        if (DataController.instance.GetCharacter(Character.Main).name == "Rau")
        {
            commandText.text = DataController.instance.tutorialCmdData.RauTutorial[commandIndex];
        }
        else
        {
            commandText.text = DataController.instance.tutorialCmdData.SpeatTutorial[commandIndex];
        }
        commandIndex++;
    }

    Coroutine fadeOut;
    // 게임 진행에 필요한 콜라이더를 활성화 시킨다. 
    public void GoNextStep()
    {
        print("스텝 확인");
        if (isGoNextStep)
        {
            // 지시문이 열려있으면 페이드 아웃
            if (commandPanel.activeSelf == true)
            {
                // 페이드아웃 호출 
                fadeOut = StartCoroutine(FadeOut());

            }

            if (progressIndex < DataController.instance.progressColliders.Length)
            {
                DataController.instance.progressColliders[progressIndex].gameObject.SetActive(true);
            }
            isGoNextStep = false; 
        }
    }

    //public void setFalse()
    //{
    //    print("확인ㅂㅂㅂㅂ");
    //    if (progressIndex < DataController.instance_DataController.progressColliders.Length)
    //    {
    //        print("확인");
    //        DataController.instance_DataController.progressColliders[progressIndex].gameObject.SetActive(false);
    //    }
    //}

    CanvasGroup canvasGroup;
    float speed = 0.7f;
    public bool finishFadeIn = false;
    private static readonly int Emotion = Animator.StringToHash("Emotion");

    IEnumerator FadeIn()
    {
        canvasGroup = commandPanel.GetComponent<CanvasGroup>();

        while (canvasGroup.alpha <= 1)
        {
            if (isExistCmdSpr)
            {
                foreach(SpriteRenderer render in sprRenderers)
                {
                    Color color = render.color;
                    color.a += Time.deltaTime * speed;
                    render.color = color;
                }
            }
            canvasGroup.alpha += Time.deltaTime * speed;

            if (canvasGroup.alpha >= 1)
            {
                finishFadeIn = true;
                StopCoroutine(fadeIn);
            }
            yield return null;
        }
    }

    IEnumerator FadeOut()
    {
        canvasGroup = commandPanel.GetComponent<CanvasGroup>();

        while (canvasGroup.alpha >= 0f)
        {
            if (isExistCmdSpr)
            {
                foreach (SpriteRenderer render in sprRenderers)
                {
                    Color color = render.color;
                    color.a -= Time.deltaTime * speed;
                    render.color = color;
                }
            }

            canvasGroup.alpha -= Time.deltaTime * speed;
            if (canvasGroup.alpha <= 0f)
            {
                commandPanel.SetActive(false);
                StopCoroutine(fadeOut);
            }
            yield return null;
        }
    }

    // 대화 관련 함수

    public void StartConversation(string jsonString)
    {
        //task인 경우 대화하는동안 task 일시 중지
        if(DataController.instance.taskData != null)
            DataController.instance.taskData.isContinue = false;
        
        //Joystick 중지
        DataController.instance.StopSaveLoadJoyStick(true);
        
        DataController.instance.dialogueData.dialogues = JsontoString.FromJsonArray<Dialogue>(jsonString);
        dialogueCnt = 0;
        if(startDialogueAction != null)
        {
            startDialogueAction();
            startDialogueAction = null;
        }
        isPossibleCnvs = false;
        dialoguePanel.SetActive(true);
        UpdateWord();
    }

    public void UpdateWord()
    {
        DialogueData dialogueData = DataController.instance.dialogueData;
        int dialogueLen = dialogueData.dialogues.Length;
        // 대화가 끝나면 선택지 부를 지 여부결정 
        if (dialogueCnt >= dialogueLen)
        {
            dialoguePanel.SetActive(false);
            DataController.instance.StopSaveLoadJoyStick(false);

            isPossibleCnvs = true;
            dialogueCnt = 0;
            if (endDialogueAction != null)
            {
                endDialogueAction();
                endDialogueAction = null;
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
            if (!string.IsNullOrEmpty(dialogueData.dialogues[dialogueCnt].anim_name))
            {
                if (charDialogueAnimator)
                {
                    Debug.Log(dialogueData.dialogues[dialogueCnt].anim_name);
                    string path = "Character_dialogue/" + dialogueData.dialogues[dialogueCnt].anim_name;
                    charDialogueAnimator.runtimeAnimatorController =
                        Resources.Load(path) as UnityEditor.Animations.AnimatorController;
                    if (charDialogueAnimator.runtimeAnimatorController != null)
                    {
                        charDialogueAnimator.GetComponent<Image>().enabled = true;
                        Debug.Log((int) dialogueData.dialogues[dialogueCnt].expression + "  " +
                                  dialogueData.dialogues[dialogueCnt].expression);
                        charDialogueAnimator.SetInteger(Emotion, ((int) dialogueData.dialogues[dialogueCnt].expression));
                    }
                    else
                    {
                        charDialogueAnimator.GetComponent<Image>().enabled = false;
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
                charDialogueAnimator.runtimeAnimatorController = null;
                charDialogueAnimator.GetComponent<Image>().enabled = false;
            }

            // 대화 텍스트 업데이트

            // 현재 실행 중인 캐릭터에 대해서
            // 현재 맵에 기준???    햄버거 집 스핏의 경우 임시로 존재하는 거니까
            // MapData에 inspector window에서 setting, position setting되어있는 캐릭터들도 추가
            // 대화 시 무슨 캐릭터인지 anim_name으로 Find (who를 사용)
            MapData.AnimationCharacterSet animator = DataController.instance.currentMap.characters.Find(
                item => item.who.ToString().Equals(dialogueData.dialogues[dialogueCnt].anim_name));
            animator?.characterAnimator.SetInteger(Emotion, (int) dialogueData.dialogues[dialogueCnt].expression);
    
            // 해당 캐릭터에 setEmotion
            // anim.SetInteger("Emotion", (int)emotion); // 애니메이션실행
            speakerName.text = dialogueData.dialogues[dialogueCnt].name; // 이야기하는 캐릭터 이름
            speakerWord.text = dialogueData.dialogues[dialogueCnt].contents; // 캐릭터의 대사
            dialogueCnt++;
        }

    }

    // 선택지가 있을 때 선택지 패널 염 
    public void OpenChoicePanel()
    {
        DataController.instance.StopSaveLoadJoyStick(true);
        TaskData currentTaskData = DataController.instance.taskData;
        int index = currentTaskData.taskIndex;
        int choiceLen = int.Parse(currentTaskData.tasks[index].nextFile);

        int[] likeable = DataController.instance.GetLikeable();

        choiceBtn[0].transform.parent.gameObject.SetActive(true);
        if (currentTaskData.tasks.Length <= index + choiceLen)
        {
            Debug.LogError("선택지 개수 오류 - IndexOverFlow");
        }
        // 선택지 개수와 조건에 맞게 선택지가 나올 수 있도록 함.
        for (int i = 0; i < choiceBtn.Length; i++)
        {
            if (choiceLen <= i)
            {
                choiceBtn[i].SetActive(false);
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
                    choiceBtn[i].SetActive(true);
                    choiceText[i].text = choiceTask.name;
                }    
            }
            else
            {
                if (condition[0] <= likeable[0] && condition[1] <= likeable[1] && condition[2] <= likeable[2])
                {
                    choiceBtn[i].SetActive(true);
                    choiceText[i].text = choiceTask.name;
                }    
            }
        }
    }
    

    public void SetChoiceAction(UnityAction<int> pressBtn)
    {
        pressBtnMethod += pressBtn;
    }
    public void SetDialougueStartAction(UnityAction unityAction)
    {
        startDialogueAction += unityAction;
    }
    public void SetDialougueEndAction(UnityAction unityAction)
    {
        endDialogueAction += unityAction;
    }
    /// <summary>
    /// 대화 선택지를 삭제하는 함수
    /// </summary>
    void RemoveChoice()
    {
        choiceBtn[0].transform.parent.gameObject.SetActive(false);
    }

    // 선택지를 눌렀을 때 불리는 함수, Index 1부터 시작
    public void PressChoice(int index)
    {
        RemoveChoice();
        DataController.instance.StopSaveLoadJoyStick(false);
        pressBtnMethod(index);
        
        // currentTaskData.taskIndex += choiceLen + 1;
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static Data.CustomEnum;

public class CanvasControl : MonoBehaviour
{
    [Header("저장 패널")]
    public Text[] saveText = new Text[3];
    public GameObject savePanel;

    // 추후 변경 
    [Header("친밀도/자존감")]
    public Text intimacyTextSpeat;
    public Text intimacyTextOun;
    public Text selfEstmText;

    [Header("대화 관련")]
    public bool isInConverstation;


    private bool isExistFile;

    [Header("이야기 진행")]
    public GameObject commandPanel;
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

    void Start()
    {
        var canvasScaler = GetComponent<CanvasScaler>();
        canvasScaler.referenceResolution = new Vector2(Screen.width, canvasScaler.referenceResolution.y);
    }
    

    public void ToTitleScene()
    {
        SceneManager.LoadScene("StartScene");

        // 새 게임 시작할 경우 새 데이터 파일 생성하도록
        // 씬 이름이 정해지면 주석 풀 예정

        //if (sceneName == "newGame")
        //{
        //    DataController.Instance.LoadCharData("NewData.json");
        //}

        DataController.instance.LoadData("Save", "NewData.json");
        // 씬 이동 시 현재 씬 이름 데이터 파일에 저장
        DataController.instance.charData.currentScene = "StartScene";
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
    public void UpdateStats()
    {
        selfEstmText.text = "자존감: " + DataController.instance.charData.selfEstm;
        intimacyTextSpeat.text = "스핏 친밀도: " + DataController.instance.charData.intimacy_spOun;
        intimacyTextOun.text = "오운 친밀도: " + DataController.instance.charData.intimacy_ounRau;
    }

    bool isExistCmdSpr; // 지시문과 같이 나올 이미지 존재 여부
    GameObject curCmdSpr; // 지시문과 같이 나올 이미지의 parent
    public SpriteRenderer[] sprRenderers; // curCmdSpr의 렌더러
    Coroutine fadeIn;
    
    Coroutine fadeOut;
    CanvasGroup canvasGroup;
    float speed = 0.7f;
    public bool finishFadeIn = false;
    
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
}

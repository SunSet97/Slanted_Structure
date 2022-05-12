using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Text;
using Data;
using static Data.CustomEnum;

public class DataController : MonoBehaviour
{
    public bool[] isExistdata = new bool[3];

    [Header("조이스틱")]
    public Joystick joyStick;
    public Vector2 inputDirection = Vector2.zero; // 조이스틱 입력 방향
    public float inputDegree = 0f; // 조이스틱 입력 크기
    public bool inputJump = false; // 조이스틱 입력 점프 판정
    public bool inputDash = false; // 조이스틱 입력 대쉬 판정
    public bool inputSeat = false;
    private bool isAlreadySave;
    private bool wasJoystickUse;


    [Header("캐릭터")]
    private CharacterManager mainChar;
    private CharacterManager rau;
    private CharacterManager oun;
    private CharacterManager speat;
    private CharacterManager speat_Adult;
    private CharacterManager speat_Child;
    private CharacterManager speat_Adolescene;

    [Header("카메라 경계값")]
    public Camera cam;
    public float min_x;
    public float max_x;
    public float min_y;
    public float max_y;
    public Vector3 camDis;
    public Vector3 camRot;
    /// <summary>
    /// 디버깅용
    /// </summary>
    public float orthgraphic_Size;

    [Header("맵")]
    public MapData[] storymaps;
    public Transform mapGenerate;
    public Transform[] progressColliders;
    public Transform currentProgress;
    public Transform commandSprite;
    public MapData currentMap;
    public string mapCode;
    public int mapIndex;//맵 방문횟수>>인게임과 시네마틱 연결하는 스크립트 생성 이후, mapIndex에 관해서 맵데이터와 데이터 컨트롤러 연결해야함.
    public string playMethod; //2D 플랫포머(2D Platformer)=Plt, 쿼터뷰(Quarter view)=Qrt, 라인트레이서(Line tracer)=Line

    // 카메라 projecton orthographic에서 perspective로 전환될 때 필요
    float originOrthoSize;
    float originCamDis_y;
    

    #region 싱글톤
    //인스턴스화
    private static DataController _instance;
    public static DataController instance
    {
        get => _instance;
    }

    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    #endregion

    void Start()
    {
        ExistsData();
        LoadData("TutorialCommandData", "RauTutorial");

        //초기화
        speat = GameObject.Find("Speat").GetComponent<CharacterManager>();
        oun = GameObject.Find("Oun").GetComponent<CharacterManager>();
        rau = GameObject.Find("Rau").GetComponent<CharacterManager>();
        speat_Adolescene = GameObject.Find("Speat_Adolescene").GetComponent<CharacterManager>();
        speat_Adult = GameObject.Find("Speat_Adult").GetComponent<CharacterManager>();
        speat_Child = GameObject.Find("Speat_Child").GetComponent<CharacterManager>();
        
        joyStick = FindObjectOfType<Joystick>();
        cam = Camera.main;
        //맵 찾아서 저장
        for (int i = 0; i < 5; i++)
        {
            MapData[] temp = Resources.LoadAll<MapData>("StoryMap/ep" + i);
            storymaps = storymaps.Concat(temp).ToArray();
        }
    }

    public CharacterManager GetCharacter(Character characterType)
    {
        CharacterManager character = null;
        switch (characterType)
        {
            case Character.Main:
                character = mainChar;
                break;
            case Character.Rau:
                character = rau;
                break;
            case Character.Oun:
                character = oun;
                break;
            case Character.Speat:
                character = speat;
                break;
            case Character.Speat_Adult:
                character = speat_Adult;
                break;
            case Character.Speat_Child:
                character = speat_Child;
                break;
            case Character.Speat_Adolescene:
                character = speat_Adolescene;
                break;
        }
        return character;
    }
    public void Cinematic()//시네마틱 씬 로드
    {
        SceneManager.LoadScene("Cinematic");
    }

    public void IngameScene()//인게임 씬 로드
    {
        SceneManager.LoadScene("Ingame_set");
    }

    #region 맵 이동

    /// <summary>
    /// 맵 바꾸는 함수
    /// </summary>
    /// <param name="mapCodeBeMove">생성되는 맵의 코드</param>
    public void ChangeMap(string mapCodeBeMove)
    {
        mapCode = $"{mapCodeBeMove:000000}"; // 맵 코드 변경

        
        //모든 캐릭터 위치 대기실로 이동
        speat.WaitInRoom();
        oun.WaitInRoom();
        rau.WaitInRoom();
        speat_Child.WaitInRoom();
        speat_Adolescene.WaitInRoom();
        speat_Adult.WaitInRoom();

        if (currentMap != null)
        {
            currentMap.DestroyMap();
        }

        currentMap = Instantiate(Array.Find(storymaps, mapData => mapData.mapCode.Equals(mapCode)), mapGenerate);
        currentMap.Initialize();
        
        SetByChangedMap();
    }

    private void SetByChangedMap()
    {
        speat.PickUpCharacter();
        oun.PickUpCharacter();
        rau.PickUpCharacter();
        speat_Adolescene.PickUpCharacter();
        speat_Adult.PickUpCharacter();
        speat_Child.PickUpCharacter();
        
        //카메라 위치와 회전
        camDis = currentMap.camDis;
        camRot = currentMap.camRot;

        // 해당되는 캐릭터 초기화
        speat.InitializeCharacter();
        oun.InitializeCharacter();
        rau.InitializeCharacter();
        speat_Adolescene.InitializeCharacter();
        speat_Adult.InitializeCharacter();
        speat_Child.InitializeCharacter();

        var temp = currentMap.positionSets;
        for (int k = 0; k < temp.Count; k++)
        {
            if (temp[k].who.Equals(Character.Speat))
            {
                if (temp[k].isMain)
                    mainChar = speat;
                speat.SetCharacter(temp[k].startPosition);
            }
            else if (temp[k].who.Equals(Character.Oun))
            {
                if (temp[k].isMain)
                    mainChar = oun;
                oun.SetCharacter(temp[k].startPosition);
            }
            else if (temp[k].who.Equals(Character.Rau))
            {
                if (temp[k].isMain)
                    mainChar = rau;
                rau.SetCharacter(temp[k].startPosition);
            }else if (temp[k].who.Equals(Character.Speat_Adolescene))
            {
                if (temp[k].isMain)
                    mainChar = speat_Adolescene;
                speat_Adolescene.SetCharacter(temp[k].startPosition);
            }else if (temp[k].who.Equals(Character.Speat_Adult))
            {
                if (temp[k].isMain)
                    mainChar = speat_Adult;
                speat_Adult.SetCharacter(temp[k].startPosition);
            }else if (temp[k].who.Equals(Character.Speat_Child))
            {
                if (temp[k].isMain)
                    mainChar = speat_Child;
                speat_Child.SetCharacter(temp[k].startPosition);
            }
        }
        //조이스틱 초기화
        InitializeJoyStick(!currentMap.isJoystickNone);
        

        // CameraMoving 컨트롤
        var cameraMoving = cam.GetComponent<Camera_Moving>();

        cameraMoving.Initialize(currentMap.isCameraMoving);


        //스카이박스 세팅
        RenderSettings.skybox = currentMap.SkyboxSetting;
        DynamicGI.UpdateEnvironment();
        //현재 맵에서 카메라 세팅

        //카메라 orthographic 컨트롤
        cam.orthographic = currentMap.isOrthographic;
        if (currentMap.isOrthographic)
        {
            orthgraphic_Size = currentMap.orthographicSize;
        }

        FindProgressCollider();

        // 조작 가능한 상태로 변경 (중력 적용)
        speat.UseJoystickCharacter();
        oun.UseJoystickCharacter();
        rau.UseJoystickCharacter();
        speat_Adolescene.UseJoystickCharacter();
        speat_Child.UseJoystickCharacter();
        speat_Adult.UseJoystickCharacter();
    }

    // 게임 진행에 필요한 콜라이더와 이미지를 획득
    void FindProgressCollider()
    {
        if (currentMap.map.transform.Find("ProgressCollider") != null)
        {
            currentProgress = currentMap.map.transform.Find("ProgressCollider");
            int curProgressCnt = currentProgress.childCount;
            progressColliders = new Transform[curProgressCnt];

            // 현재 비활성화 된 콜라이더까지 모두 찾음 
            progressColliders = currentProgress.GetComponentsInChildren<Transform>(true);

            // 지시문과 함께 나오는 이미지를 획득
            if (currentMap.map.transform.Find("CommandSprites") != null)
                commandSprite = currentMap.map.transform.Find("CommandSprites");
        }
    }
    #endregion

    /// <summary>
    /// 조이스틱 상태 초기화하는 함수
    /// </summary>
    /// <param name="isOn">JoyStick On/Off</param>
    public void InitializeJoyStick(bool isOn)
    {
        joyStick.gameObject.SetActive(isOn);
        joyStick.transform.GetChild(0).gameObject.SetActive(false);
        joyStick.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().position = default;
        joyStick.OnPointerUp();
        inputDegree = 0;
        inputDirection = default;
        inputJump = false;
    }

    public void UpdateLikeable(int[] rauLikeables)
    {
        charData.selfEstm += rauLikeables[0];
        charData.intimacy_spRau += rauLikeables[1];
        charData.intimacy_ounRau += rauLikeables[2];
    }

    public int[] GetLikeable()
    {
        int[] likable = {charData.selfEstm, charData.intimacy_spRau, charData.intimacy_ounRau};
        return likable;
    }
    /// <summary>
    /// 조이스틱 멈추고 이전 상태에 따라 키거나 끄는 함수
    /// ex) 대화 이전에 조이스틱을 사용하지 않으면 계속 사용하지 않는다.
    /// </summary>
    /// <param name="isStop">Stop여부</param>
    public void StopSaveLoadJoyStick(bool isStop)
    {
        Debug.Log(isStop +  "  " + isAlreadySave);
        //처음 실행하는 경우
        if (isStop)
        {
            if(isAlreadySave) return;
            
            isAlreadySave = true;
            wasJoystickUse = joyStick.gameObject.activeSelf;
            InitializeJoyStick(false);
        }
        // Load하는 경우
        else
        {
            Debug.Log(wasJoystickUse);
            isAlreadySave = false;
            InitializeJoyStick(wasJoystickUse);
        }
    }
    
    void FindTutorialCommand()
    {
        if (mainChar.name == "Rau")
            LoadData("TutorialCommandData", "RauTutorial");
        else
            LoadData("TutorialCommandData", "SpeatTutorial");

    }

    public CharData _charData;
    public CharData charData
    {
        get
        {
            return _charData;
        }
    }

    public TaskData taskData;

    public DialogueData dialogueData;

    public TutorialCommandData _tutorialCmdData;
    public TutorialCommandData tutorialCmdData
    {
        get
        {
            return _tutorialCmdData;
        }
    }

    //저장할 데이터의 파일이름 
    public string DataFileName;

    // 세이브 데이터를 불러오는 함수 
    // 저장된 파일이 있을 경우 기존의 파일을 가져오고 없다면 새로 생성  
    public void LoadData(string dataType, string fileName)
    {
        DataFileName = fileName;


        if (dataType == "Save")
        {

            string filePath = Application.persistentDataPath + "/" + DataFileName;

            if (File.Exists(filePath))
            {

                Debug.Log("로드 성공");
                string FromJsonData = File.ReadAllText(filePath);
                _charData = JsonUtility.FromJson<CharData>(FromJsonData);
            }
            else
            {
                print(Application.persistentDataPath);
                Debug.Log("새 파일 생성");
                _charData = new CharData();
            }
        }
        else if (dataType == "TutorialCommandData") // 튜토리얼 지시문 불러오기 
        {
            StringBuilder filePath = new StringBuilder();
            filePath.AppendFormat("{0}/Resources/DialogueScripts/ProgressCollider/{1}/{2}.json", Application.dataPath, DataFileName, dataType);
            if (File.Exists(filePath.ToString()))
            {
                Debug.Log("로드 성공");
                string FromJsonData = File.ReadAllText(filePath.ToString());
                _tutorialCmdData = JsonUtility.FromJson<TutorialCommandData>(FromJsonData);

            }
            else
            {
                print(Application.persistentDataPath);
                Debug.Log("파일 없음");
            }
        }
        else // 대화파일 불러오기
        {
            StringBuilder filePath = new StringBuilder();
            filePath.AppendFormat("{0}/Resources/DialogueScripts/{1}/{2}", Application.dataPath, dataType, DataFileName);
            //string filePath = Application.dataPath + "/Resources/DialogueScripts/" + dataType + "/" + DataFileName;
            print(filePath);
            if (File.Exists(filePath.ToString()))
            {
                Debug.Log("로드 성공");
                Debug.Log(filePath.ToString());
                dialogueData.dialogues = JsontoString.FromJsonPath<Dialogue>(filePath.ToString());
                if (dataType != "ScriptCollider")
                    instance.charData.dialogue_index++;
            }
            else
            {
                Debug.Log("기본 대사 파일");
                filePath.Clear();
                filePath.AppendFormat("{0}/Resources/DialogueScripts/{1}/Default.json", Application.dataPath, dataType);
                string FromJsonData = File.ReadAllText(filePath.ToString());
                Debug.Log(FromJsonData);
                //_dialogueData = JsonUtility.FromJson<DialogueData>(FromJsonData);
            }
        }
    }

    // 캐릭터 데이터 저장
    public void SaveCharData(string fileName)
    {
        string ToJsonData = JsonUtility.ToJson(charData);
        string filePath = Application.persistentDataPath + "/" + fileName;
        File.WriteAllText(filePath, ToJsonData);
        Debug.Log("저장");
    }

    // 세개의 슬롯에 데이터 파일이 존재하는 지 확인하는 함수
    public void ExistsData()
    {
        for (int i = 0; i < 3; i++)
        {
            string filePath = Application.persistentDataPath + "/" + "SaveData" + i;

            if (File.Exists(filePath))
            {
                isExistdata[i] = true;
            }
            else
            {
                isExistdata[i] = false;
            }
        }
    }

}

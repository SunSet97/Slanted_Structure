using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using System.Linq;

public class DataController : MonoBehaviour
{
    public bool[] isExistdata = new bool[3];
    private CanvasControl canvasCtrl;

    [Header("조이스틱")]
    public Joystick joyStick;
    public Vector2 inputDirection = Vector2.zero; // 조이스틱 입력 방향
    public float inputDegree = 0f; // 조이스틱 입력 크기
    public bool inputJump = false; // 조이스틱 입력 점프 판정
    public bool inputDash = false; // 조이스틱 입력 대쉬 판정
    public bool inputSeat = false;

    [Header("캐릭터")]
    public CharacterManager currentChar;
    public CharacterManager rau;
    public CharacterManager oun;
    public CharacterManager speat;

    [Header("카메라 경계값")]
    public Camera cam;
    public float min_x;
    public float max_x;
    public float min_y;
    public float max_y;
    public Vector3 camDis;
    public Vector3 rot;

    [Header("맵")]
    //public List<MapData> storymaps;
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
    private static DataController instance = null;
    public static DataController instance_DataController
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        if (instance)
        {
            DestroyImmediate(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    #endregion

    void Start()
    {
        ExistsData();
        LoadData("TutorialCommandData", "RauTutorial");
        //맵 찾기
        //FindMap();

        for (int i = 0; i < 5; i++)
        {
            MapData[] temp = Resources.LoadAll<MapData>("StoryMap/ep" + i);
            storymaps = storymaps.Concat(temp).ToArray();
            //Mapdata형식의 배열 storymaps에 Resources 넣어버림
            Debug.Log(storymaps.Length);
        }
    }

    void Update()
    {
        //조이스틱 찾기
        if (!joyStick) joyStick = Joystick.FindObjectOfType<Joystick>();
        //카메라 찾기
        if (!cam) cam = Camera.main;
        //캐릭터 찾기
        FindCharacter();
        InitialMap();
    }

    public void Cinematic()//시네마틱 씬 로드
    {
        SceneManager.LoadScene("Cinematic");
    }

    public void IngameScene()//인게임 씬 로드
    {
        SceneManager.LoadScene("Ingame_set");
    }
    public bool isMapChanged = false;
    //맵이 바뀔 때 초기화
    void InitialMap()
    {
        //Debug.Log(isMapChanged);
        if (isMapChanged)
        {
            speat.PickUpCharacter();
            oun.PickUpCharacter();
            rau.PickUpCharacter();
            for (int k = 0; k < currentMap.positionSets.FindAll(item => item.posSet.gameObject.activeSelf == true).Count; k++)
            {
                MapData.CharacterPositionSet temp = currentMap.positionSets.FindAll(item => item.posSet.gameObject.activeSelf == true)[k];
                if (temp.who == MapData.Character.Speat) speat.transform.position = temp.startPosition.position;
                if (temp.who == MapData.Character.Oun) oun.transform.position = temp.startPosition.position;
                if (temp.who == MapData.Character.Rau) rau.transform.position = temp.startPosition.position;
            }


            // 해당되는 캐릭터 선택
            speat.isSelected = currentMap.positionSets.Exists(item => item.who == MapData.Character.Speat);
            oun.isSelected = currentMap.positionSets.Exists(item => item.who == MapData.Character.Oun);
            rau.isSelected = currentMap.positionSets.Exists(item => item.who == MapData.Character.Rau);

            //카메라 위치와 회전
            camDis = currentMap.camDis;
            rot = currentMap.camRot;
            if (currentChar != null)
            {
                Debug.Log(camDis);
                //Debug.Log();
                cam.transform.position = currentChar.transform.position + camDis;
            }
            cam.transform.rotation = Quaternion.Euler(rot);




            // CameraMoving 컨트롤
            bool checkCameraMoving = cam.GetComponent<Camera_Moving>().enabled;
            if (currentMap.isCameraMoving && !checkCameraMoving) StartCoroutine(SetCameraMovingState(currentMap.isCameraMoving));
            else if (!currentMap.isCameraMoving && checkCameraMoving) StartCoroutine(SetCameraMovingState(currentMap.isCameraMoving));

            //스카이박스 세팅
            RenderSettings.skybox = currentMap.SkyboxSetting;
            DynamicGI.UpdateEnvironment();
            //현재 맵에서 카메라 세팅

            //카메라 orthographic 컨트롤
            cam.orthographic = currentMap.isOrthographic;
            if (currentMap.isOrthographic)
            {
                cam.orthographicSize = currentMap.orthographicSize;
            }


            FindProgressCollider();

            // 조작 가능한 상태로 변경 (중력 적용)
            speat.UseJoystickCharacter();
            oun.UseJoystickCharacter();
            rau.UseJoystickCharacter();
            isMapChanged = false;
        }
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

    void FindTutorialCommand()
    {
        if (currentChar.name == "Rau")
            LoadData("TutorialCommandData", "RauTutorial");
        else
            LoadData("TutorialCommandData", "SpeatTutorial");

    }

    //캐릭터 매니저 찾아서 정보 저장
    void FindCharacter()
    {
        if (CanvasControl.instance_CanvasControl) canvasCtrl = CanvasControl.instance_CanvasControl;
        if (!speat && GameObject.Find("Speat")) speat = GameObject.Find("Speat").GetComponent<CharacterManager>();
        if (!oun && GameObject.Find("Oun")) oun = GameObject.Find("Oun").GetComponent<CharacterManager>();
        if (!rau && GameObject.Find("Rau")) rau = GameObject.Find("Rau").GetComponent<CharacterManager>();

        if (canvasCtrl && speat && oun && rau)
        {
            if (speat.isSelected) currentChar = speat;
            if (oun.isSelected) currentChar = oun;
            if (rau.isSelected) currentChar = rau;
        }
    }

    public CharData _charData;
    public CharData charData
    {
        get
        {
            return _charData;
        }
    }

    public DialogueData _dialogueData;
    public DialogueData dialogueData
    {
        get
        {
            return _dialogueData;
        }
    }

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
            string filePath = Application.dataPath + "/Resources/DialogueScripts/ProgressCollider/" + DataFileName + "/" + dataType + ".json";
            if (File.Exists(filePath))
            {
                Debug.Log("로드 성공");
                string FromJsonData = File.ReadAllText(filePath);
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
            string filePath = Application.dataPath + "/Resources/DialogueScripts/" + dataType + "/" + DataFileName;
            print(filePath);
            if (File.Exists(filePath))
            {
                Debug.Log("로드 성공");
                string FromJsonData = File.ReadAllText(filePath);
                _dialogueData = JsonUtility.FromJson<DialogueData>(FromJsonData);
                //_dialogueData.dialogue.add dialogue
                //dialougle.addDictionary<>맵데이터에 배열로 저장
                ///for(대화 개수만큼)
                ///ArrayList.add FromJson
                ///}
                //Dictionary.add<맵코드, ArrayList>
                //dialogue.add<>
                //_dialogueData.
                if (dataType != "ScriptCollider")
                    instance.charData.dialogue_index++;
            }
            else
            {
                Debug.Log("기본 대사 파일");
                filePath = Application.dataPath + "/Resources/DialogueScripts/" + dataType + "/Default.json";

                string FromJsonData = File.ReadAllText(filePath);
                _dialogueData = JsonUtility.FromJson<DialogueData>(FromJsonData);
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

    IEnumerator SetCameraMovingState(bool isCameraMoving) {
        
        // 맵 바뀌기 전에 카메라 무빙 안되는 거 막기 위함.
        yield return new WaitUntil(() => currentChar != null && currentMap != null &&
        Mathf.Round(cam.transform.position.x) == Mathf.Round(currentChar.transform.position.x + currentMap.camDis.x) &&
        Mathf.Round(cam.transform.position.y) == Mathf.Round(currentChar.transform.position.y + currentMap.camDis.y) &&
        Mathf.Round(cam.transform.position.z) == Mathf.Round(currentChar.transform.position.z + currentMap.camDis.z));
       
        if (isCameraMoving) cam.GetComponent<Camera_Moving>().enabled = true;
        else if(!isCameraMoving) cam.GetComponent<Camera_Moving>().enabled = false;

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.SceneManagement;

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
    public List<MapData> maps;
    public Transform[] progressColliders;
    public Transform currentProgress;
    public Transform commandSprite;
    public MapData currentMap;
    public string mapCode;
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
    }

    void Update()
    {
        //조이스틱 찾기
        if (!joyStick) joyStick = Joystick.FindObjectOfType<Joystick>();
        //카메라 찾기
        if (!cam) cam = Camera.main;
        //캐릭터 찾기
        FindCharacter();
        //맵 찾기
        FindMap();

        // 카메라 projction을 orthographic으로 전환. 그리고 camDis_y = 1로 변경 (스핏 오피스텔)
        if (mapCode.Equals("002010") && cam.orthographic.Equals(false))
        {
            cam.orthographic = true;
            originOrthoSize = cam.orthographicSize;
            cam.orthographicSize = 4;
            originCamDis_y = camDis.y;
            camDis.y = 1;
        }
        /*else if (!mapCode.Equals("010101") && cam.orthographic.Equals(true))
        {
            camDis_y = originCamDis_y;
            cam.orthographicSize = originOrthoSize;
            cam.orthographic = false;
        }*/

    }

    public void Cinematic()//시네마틱 씬 로드
    {
        SceneManager.LoadScene("Cinematic");
    }

    public void IngameScene()//인게임 씬 로드
    {
        SceneManager.LoadScene("IngameScene");
    }

    public bool isMapChanged = false; // 맵 변경 여부
    //맵 코드에 맞는 맵을 찾아서 정보 저장
    void FindMap()
    {
        // 인게임 씬에서 맵 데이터 리스트 찾기 및 정렬
        if (SceneManager.GetActiveScene().name == "Ingame")
        {
            if (GameObject.FindObjectsOfType<MapData>().Length != maps.Count)
            {
                // 맵 데이터 찾기
                for (int i = 0; i < GameObject.FindObjectsOfType<MapData>().Length; i++)
                    maps.Add(GameObject.FindObjectsOfType<MapData>()[i]);
                // 맵 데이터를 다 찾으면 맵 코드 순서로 정렬
                if (GameObject.FindObjectsOfType<MapData>().Length == maps.Count)
                    maps.Sort(delegate (MapData A, MapData B) { if (int.Parse(A.mapCode) > int.Parse(B.mapCode)) return 1; else return -1; });
            }
        }
        // 맵을 찾은 경우에만 실행
        if (maps.Count > 0)
        {
            // 첫번째 맵인 000000을 제외하고 반복
            for (int i = 1; i < maps.Count; i++)
            {
                if (maps[i].mapCode == mapCode && isMapChanged)
                {
                    // 이동 가능한 상태로 변경
                    speat.PickUpCharacter();
                    oun.PickUpCharacter();
                    rau.PickUpCharacter();

                    // 해당되는 모든 캐릭터 이동
                    for (int k = 0; k < maps[i].positionSets.FindAll(item => item.posSet.gameObject.activeSelf == true).Count; k++)
                    {
                        MapData.CharacterPositionSet temp = maps[i].positionSets.FindAll(item => item.posSet.gameObject.activeSelf == true)[k];
                        if (temp.who == MapData.Character.Speat) speat.transform.position = temp.startPosition.position;
                        if (temp.who == MapData.Character.Oun) oun.transform.position = temp.startPosition.position;
                        if (temp.who == MapData.Character.Rau) rau.transform.position = temp.startPosition.position;
                    }

                    // 해당되는 캐릭터 선택
                    speat.isSelected = maps[i].positionSets.Exists(item => item.who == MapData.Character.Speat);
                    oun.isSelected = maps[i].positionSets.Exists(item => item.who == MapData.Character.Oun);
                    rau.isSelected = maps[i].positionSets.Exists(item => item.who == MapData.Character.Rau);
                    currentMap = maps[i];
                    RenderSettings.skybox = currentMap.SkyboxSetting;
                    DynamicGI.UpdateEnvironment();
                    camDis = currentMap.camDis;
                    rot = currentMap.camRot;
                    if(currentChar!=null)
                    cam.transform.position = currentChar.transform.position + camDis;
                    cam.transform.rotation = Quaternion.Euler(rot);

                    FindProgressCollider();

                    isMapChanged = false;

                    // 조작 가능한 상태로 변경 (중력 적용)
                    speat.UseJoystickCharacter();
                    oun.UseJoystickCharacter();
                    rau.UseJoystickCharacter();
                    // 나중에 주석 풀 코드
                    // 추후에 튜토리얼 맵 이름 확정되면 사용될 함수 (튜토리얼 지시문 데이터 불러오기)
                    //if (mapCode == "라우 튜토리얼 맵코드" || mapCode == "스핏튜토리얼 맵코드")
                    //    FindTutorialCommand();

                    // 디버깅용으로 일단 무조건 라우 튜토리얼 데이터 불러주기 (Start에서)
                }
            }
            //언제불리는지 확인
            //playMethod = currentMap.GetComponent<MapData>().playMethod; //플레이 방식 설정
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

}

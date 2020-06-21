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

    [Header("캐릭터")]
    public CharacterManager currentChar;
    public CharacterManager rau;
    public CharacterManager oun;
    public CharacterManager speat;

    [Header("카메라 경계값")]
    public Camera cam;
    public float min_x = -1f;
    public float max_x = 5f;
    public float min_y = -2.0f;
    public float max_y = 1.3f;
    public float camDis = 10; //캐릭터와 카메라와의 거리
    public Vector3 rot;

    [Header("맵")]
    public GameObject[] maps;
    public GameObject currentMap;
    public string mapCode;
    public string playMethod; //2D 플랫포머(2D Platformer)=Plt, 쿼터뷰(Quarter view)=Qrt, 라인트레이서(Line tracer)=Line

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

    void Start()
    {
        ExistsData();
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
        
    }

    string temp;
    public bool isMapChanged = false;
    //맵 코드에 맞는 맵을 찾아서 정보 저장
    void FindMap()
    {
        if (SceneManager.GetActiveScene().name == "Ingame")
            maps = GameObject.FindGameObjectsWithTag("Map");
        if (maps.Length > 0 && CanvasControl.instance_CanvasControl)
        {
            temp = CanvasControl.instance_CanvasControl.mapcode.textComponent.text; //디버깅용

            for (int i = 0; i < maps.Length; i++)
            {
                if (maps[i].name == temp) mapCode = temp;

                if (maps[i].name == mapCode)
                {
                    if (isMapChanged == false && currentMap != maps[i] && currentChar)
                    {
                        isMapChanged = true;
                        currentChar.transform.position = maps[i].transform.position;
                    }
                    else if (isMapChanged) isMapChanged = false;

                    currentMap = maps[i];
                    maps[i].GetComponent<MapData>().map.SetActive(true);
                }
                else
                {
                    maps[i].GetComponent<MapData>().map.SetActive(false);
                }
            }
            playMethod = currentMap.GetComponent<MapData>().playMethod; //플레이 방식 설정
        }
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
            speat.isSelected = canvasCtrl.selectedCharacter[0].isOn;
            oun.isSelected = canvasCtrl.selectedCharacter[1].isOn;
            rau.isSelected = canvasCtrl.selectedCharacter[2].isOn;

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
        else // 대화파일 불러오기
        {
            string filePath = Application.dataPath + "/Resources/DialogueScripts/" + dataType + "/" + DataFileName;

            if (File.Exists(filePath))
            {
                Debug.Log("로드 성공");
                string FromJsonData = File.ReadAllText(filePath);
                _dialogueData = JsonUtility.FromJson<DialogueData>(FromJsonData);
                instance.charData.dialogue_index++;
            }
            else
            {
                Debug.Log("기본 대사 파일");
                filePath = Application.dataPath + "/Resources/DialogueScripts/" + dataType + "/Default.json";
                print(filePath);
                string FromJsonData = File.ReadAllText(filePath);
                _dialogueData = JsonUtility.FromJson<DialogueData>(FromJsonData);
            }
        }


    }

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.SceneManagement;

//캐릭터 정보 구조체
public struct CharacterInfo
{
    public string name; //이름
    public CharacterManager char_mng; //캐릭터 매니져

    public CharacterInfo(string name, CharacterManager char_mng)
    {
        this.name = name;
        this.char_mng = char_mng;
    }
}

public class DataController : MonoBehaviour
{

    public bool[] isExistdata = new bool[3];
    CanvasControl canvasCtrl;
    GameObject player;

    [Header("카메라 경계값")]
    //public Camera cam;
    public float min_x = -1f;
    public float max_x = 5f;
    public float min_y = -2.0f;
    public float max_y = 1.3f;
    public float Z_Value = -10; //카메라 Z값 고정하기 위한 상수값

    [Header("카메라 각도")]
    //public float Rot_x;
    //public float Rot_y;
    //public float Rot_z;
    public Vector3 Rot;

    [Header("플레이 방식")]
    public string playMethod = "Plt"; //2D 플랫포머(2D Platformer)=Plt, 쿼터뷰(Quarter view)=Qrt, 라인트레이서(Line tracer)=Line

    [Header("조이스틱")]
    public Joystick joyStick; //조이스틱

    [Header("캐릭터")]
    public CharacterInfo[] char_info = new CharacterInfo[]
    {
        new CharacterInfo("Rau",null),
        new CharacterInfo("Speat",null),
        new CharacterInfo("Oun",null)
    };


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

        //캐릭터 매니저 찾아서 정보 저장
        //CharacterManager[] char_mngs = CharacterManager.FindObjectsOfType<CharacterManager>();
        //for (int i = 0; i < 3; i++)
        //    for (int j = 0; j < char_mngs.Length; j++)
        //        if (char_info[i].name == char_mngs[j].name)
        //            char_info[i].char_mng = char_mngs[j];
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

    // 데이터 저장 버튼을 누르면 불리는 함수
    public void SelectData(int fileNum)
    {
        
        player = GameObject.FindGameObjectWithTag("Player");
        

        if (canvasCtrl == null)
        {
            canvasCtrl = GameObject.Find("Canvas").GetComponent<CanvasControl>();
        }

        if (instance_DataController.charData.pencilCnt > 0)
        {
            // 기존 데이터 없을 경우 버튼 텍스트 업데이트
            if (isExistdata[fileNum] == false)
            {
                isExistdata[fileNum] = true;
                canvasCtrl.saveText[fileNum].text = "FULL DATA";
            }

            // 데이터 저장 시 연필 개수, 캐릭터 위치, 현재 씬 등 업데이트 (점점 추가할 예정)
            instance_DataController.charData.pencilCnt -= 1;
            instance_DataController.charData.endPosition = player.transform.position;
            instance_DataController.charData.currentScene = SceneManager.GetActiveScene().name;

            instance_DataController.SaveCharData("SaveData" + fileNum);

        }
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

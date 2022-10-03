﻿using UnityEngine;
using System.IO;
using System;
using CommonScript;
using Data;
using Utility.Save;
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
    public MapData currentMap;
    public string mapCode;

    // 카메라 projecton orthographic에서 perspective로 전환될 때 필요
    float originOrthoSize;
    float originCamDis_y;

    private AssetBundle _mapDB;
    private AssetBundle _materialDB;
    [NonSerialized]
    public AssetBundle dialogueDB;
    #region 싱글톤
    //인스턴스화
    private static DataController _instance;
    public static DataController instance
    {
        get => _instance;
    }

    public delegate void OnLoadMap();

    private OnLoadMap onLoadMap;
    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    private void Init()
    {
        //초기화
        speat = GameObject.Find("Speat").GetComponent<CharacterManager>();
        oun = GameObject.Find("Oun").GetComponent<CharacterManager>();
        rau = GameObject.Find("Rau").GetComponent<CharacterManager>();
        speat_Adolescene = GameObject.Find("Speat_Adolescene").GetComponent<CharacterManager>();
        speat_Adult = GameObject.Find("Speat_Adult").GetComponent<CharacterManager>();
        speat_Child = GameObject.Find("Speat_Child").GetComponent<CharacterManager>();
        
        speat.Init();
        oun.Init();
        rau.Init();
        speat_Adolescene.Init();
        speat_Adult.Init();
        speat_Child.Init();
        
        joyStick = FindObjectOfType<Joystick>();
        cam = Camera.main;
    }

    public void GameStart(string mapcode)
    {
        Init();
        mapCode = "000000";
        // var destMapCode = mapCode;
        // var loadMapCode = SaveManager.GetSaveData().mapCode;
        // Debug.Log(loadMapCode);
        // if (loadMapCode == null)
        // {
        //     Debug.Log("잘못 실행하거나 세이브 데이터 없거나");
        // }
        // else
        // {
        //     destMapCode = loadMapCode;
        // }
        ChangeMap(mapcode);
    }
    
    private MapData[] LoadMap(string desMapCode)
    {
        Debug.Log("현재 맵: " + mapCode);
        Debug.Log("다음 맵: " + desMapCode);
        var curEp = int.Parse(mapCode.Substring(0, 1));
        var desEp = int.Parse(desMapCode.Substring(0, 1));
        
        var curDay = int.Parse(mapCode.Substring(1, 2));
        var desDay = int.Parse(desMapCode.Substring(1, 2));
        
        mapCode = $"{desMapCode:000000}"; // 맵 코드 변경
        
        Debug.Log("Episode:  " + curEp + " : " + desEp);
        Debug.Log("Day:   " + curDay + " : " + desDay);
        if (curEp == desEp && curDay == desDay)
        {
            return null;
        }

        if (_mapDB != null)
        {
            _mapDB.Unload(true);
        }
        if (dialogueDB != null)
        {
            dialogueDB.Unload(true);
        }

        if (_materialDB != null)
        {
            _materialDB.Unload(true);
        }
        
        Debug.Log(Application.dataPath + $"/AssetBundles/map/ep{desEp}/day{desDay}");
        
        _mapDB = AssetBundle.LoadFromFile($"{Application.dataPath}/AssetBundles/map/ep{desEp}/day{desDay}");
        if (curDay != desDay)
        {
            dialogueDB = AssetBundle.LoadFromFile($"{Application.dataPath}/AssetBundles/dialogue/ep{desEp}");
            Debug.Log(dialogueDB);
        }
        _materialDB = AssetBundle.LoadFromFile($"{Application.dataPath}/AssetBundles/modulematerials");
        
        if (_mapDB == null)
        {
            Debug.LogError("맵 어셋 번들 오류");
            return null;
        }
        var mapDataObjects = _mapDB.LoadAllAssets<GameObject>();
        MapData[] mapDatas = new MapData[mapDataObjects.Length];
        for (int i = 0; i < mapDataObjects.Length; i++)
        {
            mapDatas[i] = mapDataObjects[i].GetComponent<MapData>();
        }
        return mapDatas;
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

    #region 맵 이동

    /// <summary>
    /// 맵 바꾸는 함수
    /// </summary>
    /// <param name="desMapCode">생성되는 맵의 코드</param>
    public void ChangeMap(string desMapCode)
    {
        //모든 캐릭터 위치 대기실로 이동
        speat.WaitInRoom();
        oun.WaitInRoom();
        rau.WaitInRoom();
        speat_Child.WaitInRoom();
        speat_Adolescene.WaitInRoom();
        speat_Adult.WaitInRoom();

        if (currentMap != null)
        {
            currentMap.gameObject.SetActive(false);
            currentMap.DestroyMap();
        }
        storymaps = LoadMap(desMapCode);

        ObjectClicker.instance.gameObject.SetActive(false);
        ObjectClicker.instance.isCustomUse = false;
        
        foreach (var item in storymaps)
        {
            Debug.Log(item);
        }
        var nextMap = Array.Find(storymaps, mapData => mapData.mapCode.Equals(mapCode));
        Debug.Log(mapCode);
        Debug.Log(nextMap);
        currentMap = Instantiate(nextMap, mapGenerate);
        currentMap.Initialize();
        SetByChangedMap();
        
        Debug.Log("22222222222222222222222222222222222");
        onLoadMap?.Invoke();
        onLoadMap = () => { };
    }

    public void AddOnLoadMap(OnLoadMap onLoadMap)
    {
        this.onLoadMap += onLoadMap;
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

        DialogueController.instance.Initialize();

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
                speat.Emotion = Expression.IDLE;
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

        if (mainChar)
        {
            mainChar.gameObject.layer = LayerMask.NameToLayer("Player");
        }

        //조이스틱 초기화
        isAlreadySave = false;
        InitializeJoyStick(!currentMap.isJoystickNone);
        

        // CameraMoving 컨트롤
        var cameraMoving = cam.GetComponent<Camera_Moving>();

        cameraMoving.Initialize();


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

        // 조작 가능한 상태로 변경 (중력 적용)
        speat.UseJoystickCharacter();
        oun.UseJoystickCharacter();
        rau.UseJoystickCharacter();
        speat_Adolescene.UseJoystickCharacter();
        speat_Child.UseJoystickCharacter();
        speat_Adult.UseJoystickCharacter();
        
        Debug.Log("333333333333333333333333333333333333333333");
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
        inputDirection = Vector2.zero;
        joyStick.input = Vector2.zero;
        inputJump = false;
    }
    /// <summary>
    /// 조이스틱 멈추고 이전 상태에 따라 키거나 끄는 함수
    /// ex) 대화 이전에 조이스틱을 사용하지 않으면 계속 사용하지 않는다.
    /// </summary>
    /// <param name="isStop">Stop여부   true - save, false - load</param>
    public void StopSaveLoadJoyStick(bool isStop)
    {
        Debug.Log((isStop ? "Save" : "Load") +  ", " + (isAlreadySave ? "저장된 상태" : "저장되지 않은 상태"));
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
            Debug.Log("저장된 상태 - " + wasJoystickUse);
            isAlreadySave = false;
            InitializeJoyStick(wasJoystickUse);
        }
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
    
    public CharData charData;

    public TaskData taskData;

    public DialogueData dialogueData;
}

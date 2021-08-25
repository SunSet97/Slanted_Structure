using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class MapData : MonoBehaviour
{
    #region Default
    public enum Character
    {
        Speat,
        Oun,
        Rau
    }
    public enum JoystickInputMethod
    {
        OneDirection,
        AllDirection,
        Other
    }
    public enum EndMap
    {
        EndingPoint,
        FinishDialogue,
        FinishTimeLine
    }

    [TextArea(7, int.MaxValue)]
    [SerializeField]
    private string scriptDesription = "맵 자동 생성 및 맵 정보 포함 스크립트이며\n인스펙터 창에서 각각의 이름에 마우스를 갖다대면 설명을 볼 수 있습니다.\n(Edit mode에서도 바로 사용 가능)";
    #endregion
    #region 맵 설정
    [Header("#Map Setting")]
    [Tooltip("맵의 코드이며 변경시 오브젝트의 이름도 같이 변경 됩니다.(코드는 반드시 6자리)")]
    public string mapCode = "000000"; // auto setting
    [Tooltip("이 맵의 조이스틱 입력 방식입니다.")]
    public JoystickInputMethod method; // 맵의 조이스틱 입력 방식
    [Tooltip("맵코드 엔딩 방식입니다.")]
    public EndMap endMap; // 맵 종료 여부
    public PlayableDirector director;
    [Tooltip("클리어시 넘어갈 다음 맵의 맵 코드입니다.")]
    public string nextMapcode = "000000"; // 클리어시 넘어갈 다음 맵의 코드
    [Space(15)]
    [Tooltip("맵의 이름은 사용자가 원하는 대로 변경하면 되며 맵 구성 어셋들은 이 오브젝트의 자식으로 설정해주면 됩니다.")]
    public GameObject map; // auto setting
    [Tooltip("이 맵의 전용 UI를 넣어주시면 됩니다.")]
    public GameObject ui; // 맵 전용 UI
    [Tooltip("이 맵의 전용 SkyBox를 넣어주시면 됩니다.")]
    public Material SkyboxSetting; // 맵 전용 스카이박스
    [Tooltip("카메라의 orthographic 뷰를 제어할 수 있습니다.")]
    public bool isOrthographic = false;
    public float orthographicSize;
    [Tooltip("CameraMoving 사용 설정")]
    public bool isCameraMoving = true; // 카메라 무빙 사용 여부
    [Tooltip("CameraMoving 경계 설정")]
    public BoxCollider cameraBound; // 카메라 무빙 경계 설정.

    // 초기 세팅 설정
    void CreateDefaultSetting()
    {
        GameObject temp = new GameObject(); // 임시 오브젝트 생성
        if (map == null) // 맵 생성 및 이름 설정
        {
            map = Instantiate(temp, transform.position, Quaternion.identity, transform);
            map.name = "Map Name";
        }
        if (positionSetting == null)// 캐릭터 위치 세팅 오브젝트 생성 및 이름 설정
        {
            positionSetting = Instantiate(temp, transform.position, Quaternion.identity, transform);
            positionSetting.name = "Position Setting";
        }
        DestroyImmediate(temp); //임시 오브젝트 제거
    }

    
    //맵 엔딩 세팅

    bool isStop = false;
    void TimeLineStop() 
    {
        if (isStop == false)
        {
            director.stopped += OnPlaybleDirectorStopped;
            isStop = true;
        }
    }
    void OnPlaybleDirectorStopped(PlayableDirector Director) //타임라인 인식
    {
        if (director == Director)//현재 Director가 맵데이터에 연결된 타임라인이라면~!
        {
            Invoke("EndingDelay", 1.5f);//타임라인 끝난거 인식한 1.5초 뒤에 함수 호출
        }
    }
    //타임라인이 멈춰있는지에 대해 인식을 받고 이에 대해 1.5초 이후 Map Clear체크 누르기!
    private void EndingDelay()
    {
        //현재 캐릭터와 맞는 포시션 세팅을 찾아서 Clear box활성화
        //positionSets.Find(item=>(item.who).ToString()==DataController.instance_DataController.currentChar.name).clearBox.GetComponent<CheckMapClear>().isClear = true;
        //방문 횟수Index에 따라서
        positionSets.Find(item => item.index == posIndex).clearBox.GetComponent<CheckMapClear>().Clear();
    }
    #endregion

    #region 위치 설정
    [Header("#Position Setting")]
    [ContextMenuItem("Remove/Rau", "RemoveRauPosition")]
    [ContextMenuItem("Remove/Oun", "RemoveOunPosition")]
    [ContextMenuItem("Remove/Speat", "RemoveSpeatPosition")]
    [ContextMenuItem("Remove/All", "RemoveAllPosition")]
    [ContextMenuItem("Create/Rau", "CreateRauPosition")]
    [ContextMenuItem("Create/Oun", "CreateOunPosition")]
    [ContextMenuItem("Create/Speat", "CreateSpeatPosition")]
    [ContextMenuItem("Create/All", "CreateAllPosition")]
    [Tooltip("인스펙터를 우클릭하여 원하는 캐릭터의 시작위치와 목표위치를 생성 및 제거하세요.")]
    [SerializeField] private GameObject positionSetting; // auto setting

    [Serializable]
    public class CharacterPositionSet
    {
        public int index;               // 시작위치 순서
        public Character who;           // 누군지
        public Transform startPosition; // 시작 위치
        public Transform clearBox;      // 클리어 박스
        public Transform posSet;        // 포지션 세팅 오브젝트
        public bool isControl;
    }

    [Tooltip("각각의 캐릭터의 시작위치와 목표위치를 설정하세요.")]
    public List<CharacterPositionSet> positionSets; // auto setting
    public int posIndex = 0; // 시작위치 순서

    [Header("Camera Setting")]
    [SerializeField] private Camera cam; // auto setting
    public Vector3 camDis;  // 캐릭터와 카메라와의 거리
    public Vector3 camRot;  // 캐릭터와 카메라와의 거리 


    // 캐릭터 위치 설정 생성
    public void CreateSpeatPosition() { CreatePositionSetting(Character.Speat); }
    public void CreateOunPosition() { CreatePositionSetting(Character.Oun); }
    public void CreateRauPosition() { CreatePositionSetting(Character.Rau); }
    public void CreateAllPosition() { CreateSpeatPosition(); CreateOunPosition(); CreateRauPosition(); }
    void CreatePositionSetting(Character createWho)
    {
        // 임시 설정 및 오브젝트 생성
        CharacterPositionSet temp = new CharacterPositionSet();
        Transform instant = new GameObject().transform;
        // 포지션 세팅 오브젝트 설정
        temp.posSet = Instantiate(instant, positionSetting.transform.position, Quaternion.identity, positionSetting.transform) as Transform;
        // 인덱스 설정
        temp.index = positionSets.Count;
        // 캐릭터 설정
        temp.who = createWho;
        // 시작 위치 설정
        temp.startPosition = Instantiate(instant, positionSetting.transform.position, Quaternion.identity, temp.posSet) as Transform;
        temp.startPosition.name = createWho.ToString() + " Start Position";
        // 클리어 박스 설정
        temp.clearBox = Instantiate(instant, positionSetting.transform.position, Quaternion.identity, temp.posSet) as Transform;
        temp.clearBox.name = createWho.ToString() + " Clear Box";
        temp.clearBox.gameObject.AddComponent<BoxCollider>();
        temp.clearBox.GetComponent<BoxCollider>().isTrigger = true;
        temp.clearBox.gameObject.AddComponent<CheckMapClear>();
        // 임시 오브젝트 제거
        DestroyImmediate(instant.gameObject);
        // 리스트에 설정 추가
        positionSets.Add(temp);
    }

    // 캐릭터 위치 설정 제거
    public void RemoveSpeatPosition() { RemovePositionSetting(Character.Speat); }
    public void RemoveOunPosition() { RemovePositionSetting(Character.Oun); }
    public void RemoveRauPosition() { RemovePositionSetting(Character.Rau); }
    public void RemoveAllPosition() { RemoveSpeatPosition(); RemoveOunPosition(); RemoveRauPosition(); }
    private void RemovePositionSetting(Character removeWho)
    {
        // 리스트의 범위안에서 해당 설정 제거
        if (positionSets.Exists(item => item.who == removeWho))
        {
            // 일치하는 설정 찾기
            CharacterPositionSet temp = positionSets.Find(item => item.who == removeWho);
            // 생성한 오브젝트 제거
            DestroyImmediate(temp.posSet.gameObject);
            // 리스트의 설정 제거
            positionSets.Remove(temp);
        }
    }

    // 포지션 세팅 업데이트
    void PositionSettingUpdate()
    {
        // 스토리 코드에 따라 이름 변경
        foreach (CharacterPositionSet Item in positionSets) Item.posSet.name = Item.index.ToString() + " Position";
        // 누구냐에 따라
        foreach (CharacterPositionSet Item in positionSets)
        {
            if (Item.startPosition) Item.startPosition.name = Item.who.ToString() + " Start Position";
            if (Item.clearBox) Item.clearBox.name = Item.who.ToString() + " Clear Box";
        }
        // 리스트 정렬
        SortPositionSets();
    }

    // 인덱스에 따라 위치 설정들 보기 편하고 일관성 있게 리스트 정렬
    void SortPositionSets()
    {
        positionSets.Sort(delegate (CharacterPositionSet A, CharacterPositionSet B) { if (A.index > B.index) return 1; else return -1; });
    }
    #endregion

    void Start()
    {
        if (Application.isPlaying)
        {
            Invoke("Init", 0.01f);
        }
        else
        {
            CreateDefaultSetting();
        }
    }
    void Update()
    {
        // Edit mode에서 업데이트
        if (!Application.isPlaying)
        {
            PositionSettingUpdate();
        }

        //타임라인 인식
        if (SceneManager.GetActiveScene().name == "Cinematic")
        {
            TimeLineStop();
        }

    }

    void Init()
    {
        DataController.instance_DataController.currentMap = this;
        // 현재 맵코드와 동일하지 않을 경우 - DataController의 MapCode로 이동
        if (!DataController.instance_DataController.mapCode.Equals(mapCode))
        {
            DataController.instance_DataController.ChangeMap(DataController.instance_DataController.mapCode);
        }
    }

    public void MapChanged()
    {
        // 오브젝트의 이름을 맵 코드로 변경
        if (name != mapCode) name = mapCode;

        // UI의 이름을 맵 이름으로 변경
        if (ui != null) ui.name = map.name;

        //UI 세팅
        if (ui != null) ui.transform.SetParent(CanvasControl.instance_CanvasControl.transform.Find("UI"));

        //ClearBox who 설정
        CharacterManager[] arr = FindObjectsOfType<CharacterManager>();
        List<CharacterManager> lists = new List<CharacterManager>(arr);
        foreach (CharacterPositionSet positionSet in positionSets)
            positionSet.clearBox.GetComponent<CheckMapClear>().who = lists.Find(item => item.name == positionSet.who.ToString());


    }

    #region 디버깅용
    [Space(40)]
    [ContextMenuItem("All Off", "AllOffDebug")]
    [ContextMenuItem("All On", "AllOnDebug")]
    [ContextMenuItem("On&Off/Position Setting", "PosOnOffDebug")]
    [ContextMenuItem("On&Off/UI", "UIOnOffDebug")]
    [ContextMenuItem("On&Off/Map", "MapOnOffDebug")]
    [Tooltip("Edit mode에서 편리한 기능을 사용할 수 있습니다.")]
    [SerializeField] private string ForDebuging = "인스펙터의 이름을 우클릭해주세요"; // 디버깅 세팅

    void MapOnOffDebug() { if (map != null) map.SetActive(!map.activeSelf); } // 맵을 키고 끔
    void UIOnOffDebug() { if (ui != null) ui.SetActive(!ui.activeSelf); } // 맵 전용 UI가 있을 경우 UI 키고 끔
    private bool isOnGizmo = true; void PosOnOffDebug() { isOnGizmo = !isOnGizmo; } // 위치 설정 기즈모를 키고 끔
    void AllOnDebug() { map.SetActive(true); positionSetting.SetActive(true); ui.SetActive(true); isOnGizmo = true; } // 전부 킴
    void AllOffDebug() { map.SetActive(false); positionSetting.SetActive(false); ui.SetActive(false); isOnGizmo = false; } // 전부 끔
    // 캐릭터별 기즈모 생성
    private void OnDrawGizmos()
    {
        if (isOnGizmo)
        {
            Color[] preset = new Color[3] { Color.red, Color.yellow, Color.blue };
            for (int i = 0; i < positionSets.Count; i++)
            {
                Gizmos.color = preset[(int)positionSets[i].who] * 0.8f;
                Gizmos.DrawCube(positionSets[i].startPosition.position + Vector3.up, new Vector3(0.3f, 0.9f, 0.3f));
                Gizmos.DrawSphere(positionSets[i].startPosition.position + Vector3.up * 0.2f, 0.2f);
                Gizmos.DrawCube(positionSets[i].clearBox.position, positionSets[i].clearBox.GetComponent<BoxCollider>().size);
            }
        }
    }
    #endregion
}

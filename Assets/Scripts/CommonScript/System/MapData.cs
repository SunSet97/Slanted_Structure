using System;
using System.Collections.Generic;
using UnityEngine;
using static Data.CustomEnum;

[ExecuteInEditMode]
public class MapData : MonoBehaviour
{
    #region Default

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
    [Tooltip("클리어시 넘어갈 다음 맵의 맵 코드입니다.")]
    public string nextMapcode = "000000"; // 클리어시 넘어갈 다음 맵의 코드
    [Space(15)]
    [Tooltip("맵의 이름은 사용자가 원하는 대로 변경하면 되며 맵 구성 어셋들은 이 오브젝트의 자식으로 설정해주면 됩니다.")]
    public GameObject map; // auto setting
    [Tooltip("이 맵의 전용 UI를 넣어주시면 됩니다.")]
    public RectTransform ui; // 맵 전용 UI
    [Tooltip("이 맵의 전용 SkyBox를 넣어주시면 됩니다.")]
    public Material SkyboxSetting; // 맵 전용 스카이박스
    [Tooltip("카메라의 orthographic 뷰를 제어할 수 있습니다.")]
    public bool isOrthographic = false;
    public float orthographicSize;
    [Tooltip("CameraMoving 경계 설정")]
    public BoxCollider cameraBound; // 카메라

    [Header("BGM입니다")]
    public AudioClip BGM;
    [Header("애니메이션 실행되는 캐릭터 넣으세요")]
    public List<AnimationCharacterSet> characters;
    
    [Header("#클리어 박스를 넣으세요")]
    [ContextMenuItem("Create ClearBox", "CreateClearBox")]
    [Tooltip("인스펙터 설정창에서 클리어박스를 생성하세요.")]
    [SerializeField] private GameObject clearBoxSetting;
    [SerializeField] private List<CheckMapClear> clearBoxList = new List<CheckMapClear>();
    
    
    [Tooltip("각각의 캐릭터의 시작위치와 목표위치를 설정하세요.")]
    public List<CharacterPositionSet> positionSets; // auto setting

    public CameraViewType cameraViewType; 
    public Vector3 camDis;  // 캐릭터와 카메라와의 거리
    public Vector3 camRot;  // 캐릭터와 카메라와의 거리 
    
    // 초기 세팅 설정
    void CreateDefaultSetting()
    {
        GameObject temp = new GameObject(); // 임시 오브젝트 생성
        if (map == null) // 맵 생성 및 이름 설정
        {
            map = Instantiate(temp, transform.position, Quaternion.identity, transform);
            map.name = "Map Name";
        }

        if (clearBoxSetting == null)
        {
            clearBoxSetting = Instantiate(temp, transform.position, Quaternion.identity, transform);
            clearBoxSetting.name = "ClearBox Setting";
        }
        if (positionSetting == null)    // 캐릭터 위치 세팅 오브젝트 생성 및 이름 설정
        {
            positionSetting = Instantiate(temp, transform.position, Quaternion.identity, transform);
            positionSetting.name = "Position Setting";
        }

        DestroyImmediate(temp); //임시 오브젝트 제거
    }
    #endregion

    #region 위치 설정
    [Header("#Position Setting")]
    [ContextMenuItem("Remove/Rau", "RemoveRauPosition")]
    [ContextMenuItem("Remove/Oun", "RemoveOunPosition")]
    [ContextMenuItem("Remove/Speat", "RemoveSpeatPosition")]
    [ContextMenuItem("Remove/SpeatAdult", "RemoveSpeatAdultPosition")]
    [ContextMenuItem("Remove/SpeatChild", "RemoveSpeatChildPosition")]
    [ContextMenuItem("Remove/SpeatAdolescene", "RemoveSpeatAdolescenePosition")]
    [ContextMenuItem("Remove/All", "RemoveAllPosition")]
    [ContextMenuItem("Create/Rau", "CreateRauPosition")]
    [ContextMenuItem("Create/Oun", "CreateOunPosition")]
    [ContextMenuItem("Create/Speat", "CreateSpeatPosition")]
    [ContextMenuItem("Create/SpeatAdult", "CreateSpeatAdultPosition")]
    [ContextMenuItem("Create/SpeatChild", "CreateSpeatChildPosition")]
    [ContextMenuItem("Create/SpeatAdolescene", "CreateSpeatAdolescenePosition")]
    [ContextMenuItem("Create/All", "CreateAllPosition")]
    [Tooltip("인스펙터를 우클릭하여 원하는 캐릭터의 시작위치와 목표위치를 생성 및 제거하세요.")]
    [SerializeField] private GameObject positionSetting; // auto setting

    [Serializable]
    public class AnimationCharacterSet
    {
        public Animator characterAnimator;
        public Character who;
    }
    [Serializable]
    public class CharacterPositionSet
    {
        public int index;               // 시작위치 순서
        public Character who;           // 누군지
        public Transform startPosition; // 시작 위치
        public Transform posSet;        // 포지션 세팅 오브젝트
        public bool isMain;
    }

    //ContextMenu 연결
    public void CreateClearBox()
    {
        Transform instant = new GameObject().transform;   
        // 클리어 박스 설정
        Transform clearBox = Instantiate(instant, positionSetting.transform.position, Quaternion.identity, clearBoxSetting.transform);
        clearBox.name = "Clear Box";
        clearBox.gameObject.AddComponent<BoxCollider>();
        clearBox.GetComponent<BoxCollider>().isTrigger = true;
        clearBox.gameObject.AddComponent<CheckMapClear>();
        // 임시 오브젝트 제거
        DestroyImmediate(instant.gameObject);
        // 리스트에 설정 추가
        clearBoxList.Add(clearBox.GetComponent<CheckMapClear>());
    }
    
    // 캐릭터 위치 설정 생성
    public void CreateSpeatPosition() { CreatePositionSetting(Character.Speat); }
    public void CreateOunPosition() { CreatePositionSetting(Character.Oun); }
    public void CreateRauPosition() { CreatePositionSetting(Character.Rau); }
    public void CreateSpeatAdolescenePosition() { CreatePositionSetting(Character.Speat_Adolescene); }
    public void CreateSpeatAdultPosition() { CreatePositionSetting(Character.Speat_Adult); }
    public void CreateSpeatChildPosition() { CreatePositionSetting(Character.Speat_Child); }
    public void CreateAllPosition() { CreateSpeatPosition(); CreateOunPosition(); CreateRauPosition();
        CreateSpeatAdolescenePosition();
        CreateSpeatAdultPosition();
        CreateSpeatChildPosition();
    }
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
        // temp.clearBox = Instantiate(instant, positionSetting.transform.position, Quaternion.identity, temp.posSet) as Transform;
        // temp.clearBox.name = createWho.ToString() + " Clear Box";
        // temp.clearBox.gameObject.AddComponent<BoxCollider>();
        // temp.clearBox.GetComponent<BoxCollider>().isTrigger = true;
        // temp.clearBox.gameObject.AddComponent<CheckMapClear>();
        // 임시 오브젝트 제거
        DestroyImmediate(instant.gameObject);
        // 리스트에 설정 추가
        positionSets.Add(temp);
    }

    // 캐릭터 위치 설정 제거
    public void RemoveSpeatPosition() { RemovePositionSetting(Character.Speat); }
    public void RemoveOunPosition() { RemovePositionSetting(Character.Oun); }
    public void RemoveRauPosition() { RemovePositionSetting(Character.Rau); }
    public void RemoveSpeatAdolescenePosition() { RemovePositionSetting(Character.Speat_Adolescene); }
    public void RemoveSpeatAdultPosition() { RemovePositionSetting(Character.Speat_Adult); }
    public void RemoveSpeatChildPosition() { RemovePositionSetting(Character.Speat_Child); }
    public void RemoveAllPosition() { RemoveSpeatPosition(); RemoveOunPosition(); RemoveRauPosition();
        RemoveSpeatAdolescenePosition();
        RemoveSpeatAdultPosition();
        RemoveSpeatChildPosition();
    }
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
            // if (Item.clearBox) Item.clearBox.name = Item.who.ToString() + " Clear Box";
        }
        // 리스트 정렬
        SortPositionSets();
    }

    // 인덱스에 따라 위치 설정들 보기 편하고 일관성 있게 리스트 정렬
    void SortPositionSets()
    {
        positionSets.Sort(delegate (CharacterPositionSet A, CharacterPositionSet B) { if (A.index > B.index) return 1; return -1; });
    }
    #endregion

    void Start()
    {
        if (Application.isPlaying)
        {
            if (DataController.instance.mapCode != mapCode)
            {
                DataController.instance.ChangeMap(DataController.instance.mapCode);
                Destroy(gameObject);
            }
            characters.RemoveAll(item => item == null);
            foreach (var t in positionSets)
            {
                characters.Add(new AnimationCharacterSet
                {
                    characterAnimator = DataController.instance.GetCharacter(t.who).anim,
                    who = t.who
                });
            }
            //if (FadeEffect.instance.isFade)
            //{
            //    StartCoroutine(FadeEffect.instance.FadeIn());
            //}
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
        else
        {
            JoystickInputUpdate();
        }
    }

    public void Initialize()
    {
        DataController.instance.currentMap = this;

        // 오브젝트의 이름을 맵 코드로 변경
        if (name != mapCode) name = mapCode;

        // UI의 이름을 맵 이름으로 변경
        if (ui != null)
        {
            ui.name = map.name;
            
            ui.SetParent(CanvasControl.instance.transform.Find("UI"));
            ui.offsetMax = new Vector2(0, 0);
            ui.offsetMin = new Vector2(0, 0);
            ui.localScale = new Vector3(1, 1, 1);
        }

        AudioController.instance.PlayBgm(BGM);
    }

    public void SetNextMapCode(string nextMapCode)
    {
        clearBoxList[0].nextSelectMapcode = nextMapCode;
    }
    public void MapClear(float waitTime)
    {
        Invoke("MapClear", waitTime);
    }
    public void MapClear()
    {
        if (clearBoxList.Count > 0)
            clearBoxList[0].Clear();
        else
            Debug.LogError("클리어 박스 세팅 오류");
    }
    
    public void DestroyMap()
    {
        Destroy(ui);
        Destroy(gameObject);    
    }
    
    #region Input 설정

    [Header("조이스틱 인풋 사용 유무, 조이스틱 존재 유무가 아님")]
    public bool isJoystickInputUse;
    [Header("조이스틱 존재 유무")]
    public bool isJoystickNone;
    private void JoystickInputUpdate()
    {
        var mainChar = DataController.instance.GetCharacter(Character.Main);
        if(mainChar == null) return;
        if (isJoystickInputUse)
        {
            JoystickInputManager.instance.JoystickInputUpdate(method);
        }

        if(mainChar.gameObject.activeSelf)
            mainChar.MoveCharacter(method);
    }
    #endregion
    
    #region 디버깅용
    [Space(40)]
    [ContextMenuItem("All Off", "AllOffDebug")]
    [ContextMenuItem("All On", "AllOnDebug")]
    [ContextMenuItem("On&Off/Position Setting", "PosOnOffDebug")]
    [ContextMenuItem("On&Off/UI", "UIOnOffDebug")]
    [ContextMenuItem("On&Off/Map", "MapOnOffDebug")]
    [Tooltip("Edit mode에서 편리한 기능을 사용할 수 있습니다.")]
    [SerializeField] private string forDebuging = "인스펙터의 이름을 우클릭해주세요"; // 디버깅 세팅

    void MapOnOffDebug() { if (map != null) map.SetActive(!map.activeSelf); } // 맵을 키고 끔
    void UIOnOffDebug() { if (ui != null) ui.gameObject.SetActive(!ui.gameObject.activeSelf); } // 맵 전용 UI가 있을 경우 UI 키고 끔
    private bool isOnGizmo = true; void PosOnOffDebug() { isOnGizmo = !isOnGizmo; } // 위치 설정 기즈모를 키고 끔
    void AllOnDebug() { map.SetActive(true); positionSetting.SetActive(true); ui.gameObject.SetActive(true); isOnGizmo = true; } // 전부 킴
    void AllOffDebug() { map.SetActive(false); positionSetting.SetActive(false); ui.gameObject.SetActive(false); isOnGizmo = false; } // 전부 끔
    // 캐릭터별 기즈모 생성
    // private void OnDrawGizmos()
    // {
    //     if (isOnGizmo)
    //     {
    //         Color[] preset = new Color[6] { Color.red, Color.yellow, Color.blue, Color.red, Color.red, Color.red };
    //         for (int i = 0; i < positionSets.Count; i++)
    //         {
    //             Gizmos.color = preset[(int)positionSets[i].who] * 0.8f;
    //             Gizmos.DrawCube(positionSets[i].startPosition.position + Vector3.up, new Vector3(0.3f, 0.9f, 0.3f));
    //             Gizmos.DrawSphere(positionSets[i].startPosition.position + Vector3.up * 0.2f, 0.2f);
    //         }
    //
    //         for (int i = 0; i < clearBoxList.Count; i++)
    //         {
    //             Gizmos.DrawCube(clearBoxList[i].gameObject.transform.position, clearBoxList[i].GetComponent<BoxCollider>().size);
    //         }
    //     }
    // }
    #endregion
}

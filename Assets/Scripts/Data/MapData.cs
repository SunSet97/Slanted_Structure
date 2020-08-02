using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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

    [TextArea]
    [SerializeField]
    private string scriptDesription = "맵 자동 생성 및 맵 정보 포함 스크립트이며\n인스펙터 창에서 각각의 이름에 마우스를 갖다대면 설명을 볼 수 있습니다.\n(Edit mode에서도 바로 사용 가능)";
    #endregion

    #region 맵 설정
    [Header("#Map Setting")]
    [Tooltip("맵의 코드이며 변경시 오브젝트의 이름도 같이 변경 됩니다.(코드는 반드시 6자리)")]
    public string mapCode = "000000"; // auto setting
    [Tooltip("맵의 이름은 사용자가 원하는 대로 변경하면 되며 맵 구성 어셋들은 이 오브젝트의 자식으로 설정해주면 됩니다.")]
    public GameObject map; // auto setting
    [Tooltip("이 맵의 전용 UI를 넣어주시면 됩니다.")]
    public GameObject ui; // 맵 전용 UI
    [Tooltip("이 맵의 클리어 여부입니다.")]
    public bool isMapClear; // 맵 종료 여부

    // 초기 세팅 설정
    void CreateDefaultSetting()
    {
        GameObject temp = new GameObject(); // 임시 오브젝트 생성
        if (map == null) // 맵 생성 및 이름 설정
        {
            map = Instantiate(temp, this.transform.position, Quaternion.identity, this.transform) as GameObject;
            map.name = "Map Name";
        }
        if (positionSetting == null)// 캐릭터 위치 세팅 오브젝트 생성 및 이름 설정
        {
            positionSetting = Instantiate(temp, this.transform.position, Quaternion.identity, this.transform) as GameObject;
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
        public Character who;           // 누군지
        public Transform startPosition; // 시작 위치
        public Transform clearBox;      // 클리어 박스
    }

    [Tooltip("각각의 캐릭터의 시작위치와 목표위치를 설정하세요.")]
    public List<CharacterPositionSet> positionSets; // auto setting

    // 위치 설정들 보기 편하고 일관성 있게 리스트 정렬
    void SortPositionSets()
    {
        positionSets.Sort(delegate (CharacterPositionSet A, CharacterPositionSet B) { if ((int)A.who > (int)B.who) return 1; else return -1; });
    }

    // 캐릭터 위치 설정 생성
    void CreateSpeatPosition() { CreatePositionSetting(Character.Speat); }
    void CreateOunPosition() { CreatePositionSetting(Character.Oun); }
    void CreateRauPosition() { CreatePositionSetting(Character.Rau); }
    void CreateAllPosition() { CreateSpeatPosition(); CreateOunPosition(); CreateRauPosition(); }
    void CreatePositionSetting(Character createWho)
    {
        // 리스트의 범위안에서 중복되지 않게 생성
        if ((positionSets.Count < 3 && positionSets.Count >= 0) && !positionSets.Exists(item => item.who == createWho))
        {
            // 임시 설정 및 오브젝트 생성
            CharacterPositionSet temp = new CharacterPositionSet();
            Transform instant = new GameObject().transform;
            // 캐릭터 설정
            temp.who = createWho;
            // 시작 위치 설정
            temp.startPosition = Instantiate(instant, positionSetting.transform.position, Quaternion.identity, positionSetting.transform) as Transform;
            temp.startPosition.name = createWho.ToString() + " Start Position";
            // 클리어 박스 설정
            temp.clearBox = Instantiate(instant, positionSetting.transform.position, Quaternion.identity, positionSetting.transform) as Transform;
            temp.clearBox.name = createWho.ToString() + " Clear Box";
            temp.clearBox.gameObject.AddComponent<BoxCollider>();
            temp.clearBox.GetComponent<BoxCollider>().isTrigger = true;
            temp.clearBox.gameObject.AddComponent<CheckMapClear>();
            temp.clearBox.GetComponent<CheckMapClear>().mask = (int)createWho;
            // 임시 오브젝트 제거
            DestroyImmediate(instant.gameObject);
            // 리스트에 설정 추가
            positionSets.Add(temp);
            // 리스트 정렬
            SortPositionSets();
        }
    }

    // 캐릭터 위치 설정 제거
    void RemoveSpeatPosition() { RemovePositionSetting(Character.Speat); }
    void RemoveOunPosition() { RemovePositionSetting(Character.Oun); }
    void RemoveRauPosition() { RemovePositionSetting(Character.Rau); }
    void RemoveAllPosition() { RemoveSpeatPosition(); RemoveOunPosition(); RemoveRauPosition(); }
    void RemovePositionSetting(Character removeWho)
    {
        // 리스트의 범위안에서 해당 설정 제거
        if (positionSets.Exists(item => item.who == removeWho))
        {
            // 일치하는 설정 찾기
            CharacterPositionSet temp = positionSets.Find(item => item.who == removeWho);
            // 생성한 오브젝트 제거
            DestroyImmediate(temp.startPosition.gameObject);
            DestroyImmediate(temp.clearBox.gameObject);
            // 리스트의 설정 제거
            positionSets.Remove(temp);
            // 리스트 정렬
            SortPositionSets();
        }
    }
    #endregion

    // 아직 사용 미정
    [Header("Camera Setting")]
    [SerializeField] private Camera cam; // auto setting
    public Vector3 camDis;  // 캐릭터와 카메라와의 거리
    public Vector3 camRot;  // 캐릭터와 카메라와의 거리 
    public string playMethod; // 아직 지우면 안됨 

    [Space(40)]
    [ContextMenuItem("All Off", "AllOffDebug")]
    [ContextMenuItem("All On", "AllOnDebug")]
    [ContextMenuItem("On&Off/Position Setting", "PosOnOffDebug")]
    [ContextMenuItem("On&Off/UI", "UIOnOffDebug")]
    [ContextMenuItem("On&Off/Map", "MapOnOffDebug")]
    [Tooltip("Edit mode에서 편리한 기능을 사용할 수 있습니다.")]
    [SerializeField] private string ForDebuging = "인스펙터의 이름을 우클릭해주세요"; // 디버깅 세팅

    void Start()
    {
        cam = Camera.main;
        CreateDefaultSetting();
        positionSets.Capacity = 3; // 최대 할당량은 3 (캐릭터가 최대 3개이므로)
    }


    void Update()
    {
        if (this.name != mapCode) this.name = mapCode;   // 오브젝트의 이름을 맵 코드로 변경
        if (ui != null) ui.name = map.name;             // UI의 이름을 맵 이름으로 변경
        if (mapCode.Length == 6) this.transform.position = new Vector3(int.Parse(mapCode) / 10000 * 500, (int.Parse(mapCode) / 100 - int.Parse(mapCode) / 10000 * 100) * 500, (int.Parse(mapCode) - int.Parse(mapCode) / 100 * 100) * 500);

        // 게임 플레이시 맵 정보들은 현재 맵코드에 따라 On Off
        if (EditorApplication.isPlaying && DataController.instance_DataController.mapCode == mapCode)
        {
            if (map != null) map.SetActive(DataController.instance_DataController.mapCode == mapCode);
            positionSetting.SetActive(map.activeSelf);
            if (ui != null) ui.SetActive(map.activeSelf);
            isMapClear = positionSets.Exists(item => item.clearBox.GetComponent<CheckMapClear>().isClear); // 클리어 판정인게 하나라도 있으면 맵 클리어 처리
        }
    }

    #region 디버깅용
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

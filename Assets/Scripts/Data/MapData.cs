using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapData : MonoBehaviour
{
    [Header("Character Info")]
    public bool isSpeatExist;   // 스핏 존재 여부
    public bool isOunExist;     // 오운 존재 여부
    public bool isRauExist;     // 라우 존재 여부

    [Header("Map Info")]
    public string mapCode;      // auto setting
    [ContextMenuItem("Map On Off", "MapOnOffDebug")]
    [Tooltip("게임중이 아닐때 맵울 키고 끌 수 있습니다.")]
    public GameObject map;      // auto setting
    public bool mapEnding;      // 맵 종료 여부

    [Header("Position Info")]
    public Transform speatStartPos; // 맵 이동시 스핏 시작 위치
    public Transform speatEndPos;   // 맵 이동시 스핏 종료 위치

    public Transform ounStartPos;   // 맵 이동시 오운 시작 위치
    public Transform ounEndPos;     // 맵 이동시 오운 종료 위치

    public Transform rauStartPos;   // 맵 이동시 라우 시작 위치
    public Transform rauEndPos;     // 맵 이동시 라우 종료 위치

    [Header("UI")]
    [ContextMenuItem("UI On Off", "MapUIOnOffDebug")]
    [Tooltip("게임중이 아닐때 UI를 키고 끌 수 있습니다.")]
    public GameObject ui;

    [Header("Camera Setting")]
    [SerializeField] private Camera cam; // auto setting
    public Vector3 camDis;  // 캐릭터와 카메라와의 거리
    public Vector3 camRot;  // 캐릭터와 카메라와의 거리    

    public string playMethod; // 아직 지우면 안됨

    void Start()
    {
        mapCode = name;
        map = GetComponentsInChildren<Transform>()[1].gameObject;
        cam = Camera.main;
    }

    void Update()
    {
        MapUIOnOff();
    }

    // 맵 전용 UI가 있을 경우 UI 키고 끔
    private void MapUIOnOff() { if (ui != null) ui.SetActive(map.activeSelf); }

    private void MapOnOffDebug() { map.SetActive(!map.activeSelf); } // 맵을 키고 끔 (디버깅)
    private void MapUIOnOffDebug() { if (ui != null) ui.SetActive(!ui.activeSelf); } // 맵 전용 UI가 있을 경우 UI 키고 끔(디버깅)

    // 캐릭터별 시작 위치에 원형 기즈모 생성
    private void OnDrawGizmos()
    {

        if (isSpeatExist)
        {
            Gizmos.color = Color.red * 0.8f;
            Gizmos.DrawCube(speatStartPos.position + Vector3.up, new Vector3(0.3f, 0.9f, 0.3f));
            Gizmos.DrawSphere(speatStartPos.position + Vector3.up * 0.2f, 0.2f);
            Gizmos.DrawCube(speatEndPos.position, speatEndPos.GetComponent<BoxCollider>().size);
        }

        if (isOunExist)
        {
            Gizmos.color = Color.yellow * 0.8f;
            Gizmos.DrawCube(ounStartPos.position + Vector3.up, new Vector3(0.3f, 0.9f, 0.3f));
            Gizmos.DrawSphere(ounStartPos.position + Vector3.up * 0.2f, 0.2f);
            Gizmos.DrawCube(ounEndPos.position, ounEndPos.GetComponent<BoxCollider>().size);
        }

        if (isRauExist)
        {
            Gizmos.color = Color.blue * 0.8f;
            Gizmos.DrawCube(rauStartPos.position + Vector3.up, new Vector3(0.3f, 0.9f, 0.3f));
            Gizmos.DrawSphere(rauStartPos.position + Vector3.up * 0.2f, 0.2f);
            Gizmos.DrawCube(rauEndPos.position, rauEndPos.GetComponent<BoxCollider>().size);
        }
    }
    
}

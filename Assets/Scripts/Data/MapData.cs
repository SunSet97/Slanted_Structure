using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapData : MonoBehaviour
{
    [Header("Map Info")]
    public string mapCode;      // auto setting
    public GameObject map;      // auto setting
    public Transform startPos;  // 맵 이동시 캐릭터 시작 위치

    [Header("UI")]
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
    private void MapUIOnOff()
    {
        if (ui != null) ui.SetActive(map.activeSelf);
    }

    // 시작 위치에 원형 기즈모 생성
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red * 0.8f;
        Gizmos.DrawCube(startPos.position + Vector3.up, new Vector3(0.3f, 0.9f, 0.3f));
        Gizmos.DrawSphere(startPos.position + Vector3.up * 0.2f, 0.2f);
    }
}

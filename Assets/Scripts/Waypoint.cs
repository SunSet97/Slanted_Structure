using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    private Joystick joyStick; //조이스틱
    private Transform character; //선택된 캐릭터
    private Transform[] waypointArray; //waypoint 배열

    public float checkingDistance = 0.3f; //check 가능한 거리
    public Vector3 moveTo = Vector3.zero; //waypoint간 이동 방향 벡터

    private int wayIndex; //check된 포인트 순서
    public Transform checkedWaypoint = null; //check된 포인트
    private bool isInPoint = false; //check된 포인트와 닿아있는지
    private bool isInit = false; //check된 포인트에 처음 접한것 판단
    private bool isRight = true; //이동방향 설정(횡스크롤이므로 좌,우만 존재)
    private NPCInteractor npcInteractor;
    
    void Start()
    {
        npcInteractor = GameObject.Find("NPCManager").GetComponent<NPCInteractor>();
        waypointArray = GetComponentsInChildren<Transform>();
        DisablePointMeshrenderer();
    }
    
    void Update()
    {
        //조이스틱 설정
        if (!joyStick) joyStick = DataController.instance_DataController.joyStick;
        //움직일 캐릭터 설정
        //for (int i = 0; i < 3; i++)
        //    if (SceneInformation.instance_SceneInformation.char_info[i].char_mng.isSelected)
        //    {
                //character = SceneInformation.instance_SceneInformation.char_info[i].char_mng.transform;
                character = npcInteractor.player.transform;
            //}
       if (checkedWaypoint == null)
           GetNearestWaypoint();

        GetCheckedWaypoint();

        if (checkedWaypoint != null)
            SetMoveDirection();
    }
    
    //모든 waypoint의 meshrenderer를 끄는 함수(scene 구성시에는 보이는게 편하고 ingame에서는 안보이는게 편하므로)
    private void DisablePointMeshrenderer()
    {
        foreach (Transform waypoint in waypointArray)
        {
            if (waypoint.name == "WaypointManager") continue;
            waypoint.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    //수평 거리 계산 함수
    private float HorizontalDistance(Transform from, Transform to)
    {
        Vector3 fromPos = from.position;
        Vector3 toPos = to.position;

        //각각의 수직 좌표 무시
        fromPos.y = 0;
        toPos.y = 0;

        return Vector3.Distance(fromPos, toPos); //거리 반환
    }

    //플레이어와 가장 가까운 waypoint를 찾는 함수
    private Transform GetNearestWaypoint()
    {
        float minDis = float.MaxValue; //최단거리

        //길이가 1 이하면 waypoint가 존재하지 않음
        if (waypointArray.Length <= 1) return null;

        //check가능한 waypoint를 확인하고 해당 checked point를 업데이트
        int i = 0; //순서
        foreach (Transform waypoint in waypointArray)
        {
            //Waypoint Manager는 스킵
            if (waypoint.name == "WaypointManager") continue;

            float dist = Vector3.Distance(character.position, waypoint.position); //플레이어와 waypoint간 거리
            i++; //순서 증가

            //플레이어와 waypoint의 거리가 최소 거리가 되면 해당 포인트를 check해줌
            if (dist <= minDis)
            {
                minDis = dist;
                wayIndex = i; //현재 waypoint의 순서
                checkedWaypoint = waypoint; //check한 waypoint 업데이트
            }
        }
        return checkedWaypoint;
    }

    //플레이어가 지나간(check한) waypoint를 찾는 함수
    private Transform GetCheckedWaypoint()
    {
        //길이가 1 이하면 waypoint가 존재하지 않음
        if (waypointArray.Length <= 1) return null;

        //check가능한 waypoint를 확인하고 해당 checked point를 업데이트
        int i = 0; //순서
        foreach (Transform waypoint in waypointArray)
        {
            //Waypoint Manager는 스킵
            if (waypoint.name == "WaypointManager") continue;

            float dist = HorizontalDistance(character, waypoint); //플레이어와 waypoint간 거리
            i++; //순서 증가

            //플레이어와 waypoint의 거리가 check 가능한 거리가 되면 해당 포인트를 check해줌
            if (dist < checkingDistance)
            {
                wayIndex = i; //현재 waypoint의 순서
                checkedWaypoint = waypoint; //check한 waypoint 업데이트
            }
        }
        return checkedWaypoint;
    }

    //이동 방향 설정 함수
    private void SetMoveDirection()
    {
        float dist = HorizontalDistance(character, checkedWaypoint); //플레이어와 check포인트간 거리

        //조이스틱 방향 설정
        if (joyStick.Horizontal > 0)
            isRight = true;
        else if(joyStick.Horizontal < 0)
            isRight = false;

        //check가능한 거리 내에 있으면 닿아있는 것으로 판단
        if (dist < checkingDistance) isInPoint = true;

        //point와 닿아있을 경우
        if (isInPoint)
        {
            Vector3 moveDir = character.GetComponent<CharacterManager>().moveHorDir; //플레이어의 수평 이동 속도
            float moveSpeed = Mathf.Sqrt(moveDir.x * moveDir.x + moveDir.z * moveDir.z); //플레이어의 수평 이동 속도 크기

            //처음 접했을 때
            if (!isInit)
            {
                //이동 방향 및 속도 초기화
                if (moveSpeed != 0) moveDir = Vector3.zero;
                isInit = true; //처음 닿았음 판단
            }

            //이후 지속적으로 닿아있는 상태에서 어느 방향으로 이동할 것인지 판단
            //포인트에서 이전에 이동하던 방향이 우측방향일 경우
            if (isRight)
            {
                //가장 우측에 위치한 포인트가 아닐경우 우측방향 설정
                if (wayIndex != waypointArray.Length - 1)
                    moveTo = waypointArray[wayIndex + 1].position - waypointArray[wayIndex].position;
                //가장 우측 포인트일 경우 더는 우측으로 이동 못하도록 설정
                else
                    moveTo = Vector3.zero;
            }
            //포인트에서 이전에 이동하던 방향이 좌측방향일 경우
            else if (!isRight)
            {
                //가장 좌측에 위치한 포인트가 아닐경우 좌측방향 설정
                if (wayIndex != 1)
                    moveTo = waypointArray[wayIndex].position - waypointArray[wayIndex - 1].position;
                //가장 좌측 포인트일 경우 더는 좌측으로 이동 못하도록 설정
                else
                    moveTo = Vector3.zero;
            }

            moveTo.y = 0; //수직방향 무시
            moveTo.Normalize(); //단위벡터로 변환

            //이동하는 방향에 따라 꺽어지는 부분에서 속도를 유지하도록 함
            if (isRight)
                character.GetComponent<CharacterManager>().moveHorDir = moveTo * moveSpeed;
            else
                character.GetComponent<CharacterManager>().moveHorDir = -moveTo * moveSpeed;

            isInPoint = false; //이동 후 벗어나게되면 닿아있음 해제
        }
        else
            isInit = false; //닿아있음 벗어나면 처음 접할 수 있는 상태이므로 false로 만들어줌
    }


    private void OnDrawGizmos()
    {
       
        //waypoint gizmo 표시
        if (waypointArray.Length >= 0)
        {
            foreach (Transform waypoint in waypointArray)
            {
                //매니저는 스킵
                if (waypoint.name == "WaypointManager") continue;

                //체크포인트는 녹색
                if (waypoint == checkedWaypoint)
                {
                    Gizmos.color = new Color(0, 1, 0, 0.8f);
                    Gizmos.DrawSphere(checkedWaypoint.position, 0.7f);
                    if (wayIndex != waypointArray.Length - 1) Gizmos.DrawLine(checkedWaypoint.position, waypointArray[wayIndex + 1].position);
                    if (wayIndex != 1) Gizmos.DrawLine(checkedWaypoint.position, waypointArray[wayIndex - 1].position);
                }
                //나머지 빨간색
                else
                {
                    Gizmos.color = new Color(1, 0, 0, 0.8f);
                    Gizmos.DrawSphere(waypoint.position, 0.7f);
                }
            }
        }
    }


}

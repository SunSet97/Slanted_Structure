using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class Waypoint : MonoBehaviour
{
    // 입력 방향 설정
    public enum MoveDirection
    {
        Right,
        Left
    }

    [Header("#Map Setting")]
    [Tooltip("이 맵의 MapData.cs를 넣어주세요.")]
    public MapData mapdata; // auto setting
    public Transform character; // auto setting
    [Tooltip("웨이 포인트의 전진 방향을 설정해주세요.")]
    public MoveDirection moveDirection = MoveDirection.Right; // 웨이포인트 이동방향 세팅
    public MoveDirection moveTo = MoveDirection.Right; // 이전에 이동 입력한 방향
    public Vector3 fwdDir = Vector3.zero; // 전진 방향
    public Vector3 bwdDir = Vector3.zero; // 후진 방향
    public Vector2 changedDir = Vector2.zero; // waypoint에 따른 이동 방향을 계산

    [Header("#Waypoint Setting")]
    [Tooltip("포인트 체크 가능 거리 설정입니다.")]
    public float checkingDist = 0.7f; // check 가능한 거리
    [Tooltip("체크 포인트의 인덱스입니다.")]
    public int idx = 0; // check 포인트의 배열 숫자
    [Tooltip("웨이 포인트들의 리스트입니다.")]
    public List<Transform> waypoints; // waypoint 리스트
    private Transform nearestPoint = null; // 최단 포인트
    public Transform checkedWaypoint = null; //check된 포인트
    public bool isInit = false; // check된 포인트에 처음 접한것 판단
    public bool isCheck = false;
    void Update()
    {
        // Edit / Play mode에서 업데이트
        EditSettingUpdate();
        // Play mode에서만 업데이트
        PlaySettingUpdate(Application.isPlaying);

    }

    #region 에디트, 맵데이터 세팅
    void EditSettingUpdate()
    {
        if (!mapdata) mapdata = GetComponentInParent<MapData>();
        UpdateWaypoints();
    }
    // waypoint 리스트 업데이트
    void UpdateWaypoints()
    {
        if (waypoints.Count != GetComponentsInChildren<Transform>().Length - 1)
        {
            waypoints.AddRange(GetComponentsInChildren<Transform>()); // 리스트 추가
            waypoints.RemoveAt(0); // 부모 오브젝트 제거
            waypoints.Sort(delegate (Transform A, Transform B) { if (A.name.CompareTo(B.name) > 0) return 1; else return -1; }); // 이름순으로 정렬
            checkedWaypoint = waypoints[0];
        }
    }
    #endregion

    #region 게임 플레이 세팅 업데이트
    void PlaySettingUpdate(bool isPlaying)
    {
        if (isPlaying && mapdata.map != null)
        {
            SelectCharacter();
            WaypointCheck(waypoints.Count > 0);
            JoystickInputSetting();
        }
    }
    // waypoint를 따라 움직일 캐릭터 설정
    void SelectCharacter()
    {
        if (!character)
        {
            character = DataController.instance_DataController.currentChar.transform;
        }
    }

    // 캐릭터가 waypoint를 지나가면 체크
    void WaypointCheck(bool isOn)
    {
        if (checkedWaypoint == null) checkedWaypoint = waypoints[0]; // 캐릭터와 waypoint의 거리가 check 가능한게 없으면 1번째 포인트를 check해줌
        idx = waypoints.FindIndex(item => item == checkedWaypoint); // 현재 check된 포인트의 인덱스 반환
        float minDist = float.MaxValue; // 최단 거리
        nearestPoint = waypoints[idx]; minDist = Vector3.Distance(character.position, waypoints[idx].position); // 현재 체크 포인트의 거리를 계산후 최단거리 설정
        if (idx == 0) // 다음 포인트만 비교
        {
            if (Vector3.Distance(character.position, waypoints[idx + 1].position) <= minDist) nearestPoint = waypoints[idx + 1];
        }
        else if (idx == waypoints.Count - 1) // 이전 포인트만 비교
        {
            if (Vector3.Distance(character.position, waypoints[idx - 1].position) <= minDist) nearestPoint = waypoints[idx - 1];
        }
        else // 앞뒤 포인트 둘다 비교
        {
            if (Vector3.Distance(character.position, waypoints[idx + 1].position) <= minDist) { nearestPoint = waypoints[idx + 1]; minDist = Vector3.Distance(character.position, waypoints[idx + 1].position); }
            if (Vector3.Distance(character.position, waypoints[idx - 1].position) <= minDist) { nearestPoint = waypoints[idx - 1]; minDist = Vector3.Distance(character.position, waypoints[idx - 1].position); }
        }

        Vector3 charVec = character.position, pointVec = nearestPoint.position; // 수평거리 계산을 위한 벡터
        charVec.y = 0; pointVec.y = 0; // 수직 좌표 무시
        if (Vector3.Distance(charVec, pointVec) <= checkingDist) checkedWaypoint = nearestPoint; // 캐릭터와 waypoint의 거리가 check 가능한 거리가 되면 해당 포인트를 check해줌
    }

    // 입력 방향 세팅
    void JoystickInputSetting()
    {
        if (DataController.instance_DataController.joyStick != null)
        {
            Vector2 inputDir = Vector2.zero; // 조이스틱 자체의 입력 방향
            inputDir = new Vector2(DataController.instance_DataController.joyStick.Horizontal, 0); // 한 방향 입력은 수평값만 받음

            // 움직였던 방향
            if (moveDirection == MoveDirection.Right && isInit)
                moveTo = inputDir.x > 0 ? MoveDirection.Right : inputDir.x < 0 ? MoveDirection.Left : moveTo; // 오른쪽이 전진 방향, 왼쪽이 후진 방향
            else if (moveDirection == MoveDirection.Left && isInit)
                moveTo = inputDir.x < 0 ? MoveDirection.Left : inputDir.x > 0 ? MoveDirection.Right : moveTo; // 왼쪽이 전진 방향, 오른쪽이 후진 방향
            
            // check된 waypoint가 없을 경우
            if (checkedWaypoint == null)
            {
                character.transform.position = waypoints[0].position; // 첫번째 웨이포인트 위치로 이동
            }
            else
            {
                // 전진, 후진 방향 벡터 설정
                // check가능한 거리 내에 있으면 닿아있는 것으로 판단
                isCheck = Vector3.Distance(character.position, checkedWaypoint.position) <= checkingDist;
                if (Vector3.Distance(character.position, checkedWaypoint.position) <= checkingDist)
                {
                    if (!isInit)
                    {
                        isInit = true; // 처음 닿았음 판단
                    }
                    if (idx < waypoints.Count - 1) fwdDir = waypoints[idx + 1].position - character.transform.position; // 마지막 포인트가 아닐 경우 전진 방향 설정
                    else fwdDir = waypoints[waypoints.Count - 1].position - character.transform.position; // 마지막 포인트일 경우 마지막 포인트로 향하도록 전진 방향 설정

                    if (idx > 0) bwdDir = waypoints[idx - 1].position - character.transform.position; // 첫번째 포인트가 아닐 경우 후진 방향 설정
                    //else bwdDir = waypoints[idx].position - character.transform.position; // 첫번째 포인트일 경우 첫번째 포인트로 향하도록 후진 방향 설정
                    else bwdDir = -fwdDir;
                }
                else
                {
                    isInit = false; // 닿아있음 벗어나면 처음 접할 수 있는 상태이므로 false로 만들어줌
                    if (moveTo == moveDirection) // 이동 방향과 입력 방향이 같을때
                    {
                        if (idx < waypoints.Count - 1) fwdDir = waypoints[idx + 1].position - character.transform.position; // 다음 포인트로 가게 함
                        bwdDir = waypoints[idx].position - character.transform.position; // 체크 포인트로 돌아가게 함
                    }
                    else // 이동 방향과 입력 방향이 다를때
                    {
                        fwdDir = waypoints[idx].position - character.transform.position; // 체크 포인트로 돌아가게 함
                        if (idx > 0) bwdDir = waypoints[idx - 1].position - character.transform.position; // 이전 포인트로 가게 함
                    }
                }
            }
            Quaternion camRotation = Quaternion.Euler(0, -Camera.main.transform.rotation.eulerAngles.y, 0);
            fwdDir = camRotation * fwdDir; bwdDir = camRotation * bwdDir;


            // 미리 설정해둔 입력 방향에 따라 실제 조이스틱 입력 방향으로 전방향, 후방향 벡터를 수정된 조이스틱 입력 방향으로 설정
            if (moveDirection == MoveDirection.Right)
                changedDir = inputDir.x > 0 ? new Vector2(fwdDir.x, fwdDir.z).normalized : inputDir.x < 0 ? new Vector2(bwdDir.x, bwdDir.z).normalized : Vector2.zero; // 오른쪽이 전진 방향, 왼쪽이 후진 방향
            else if (moveDirection == MoveDirection.Left)
                changedDir = inputDir.x < 0 ? new Vector2(fwdDir.x, fwdDir.z).normalized : inputDir.x > 0 ? new Vector2(bwdDir.x, bwdDir.z).normalized : Vector2.zero; // 왼쪽이 전진 방향, 오른쪽이 후진 방향
                       
            DataController.instance_DataController.inputJump = DataController.instance_DataController.joyStick.Vertical > 0.5f; // 수직 입력이 일정 수치 이상 올라가면 점프 판정
            DataController.instance_DataController.inputDirection = changedDir; // 조정된 입력 방향 설정
            DataController.instance_DataController.inputDegree = Vector2.Distance(Vector2.zero, changedDir) * Mathf.Abs(inputDir.x); // 조정된 입력 방향으로 크기 계산
        }
    }
    #endregion

    #region 디버깅용
    private void OnDrawGizmos()
    {
        // 각각의 포인트 표시(체크는 녹색, 이외 적색)
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (checkedWaypoint == waypoints[i])
            {
                Gizmos.color = Color.green * 0.7f;
                Gizmos.DrawSphere(waypoints[i].position, 0.5f);
                if (i == 0) Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                else if (i == waypoints.Count - 1) Gizmos.DrawLine(waypoints[i - 1].position, waypoints[i].position);
                else { Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position); Gizmos.DrawLine(waypoints[i - 1].position, waypoints[i].position); }
            }
            else
            {
                Gizmos.color = Color.red * 0.7f;
                Gizmos.DrawSphere(waypoints[i].position, 0.5f);
                if (i != waypoints.Count - 1) Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }

        // 가장 가까운 포인트 표시
        if (nearestPoint != null)
        {
            Gizmos.color = Color.blue * 0.7f;
            Gizmos.DrawWireSphere(nearestPoint.position, 0.5f);
        }
    }
    #endregion
}

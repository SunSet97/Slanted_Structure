using System;
using System.Collections.Generic;
using UnityEngine;
using Utility.Core;
using static Data.CustomEnum;

namespace CommonScript
{
    [ExecuteInEditMode]
    public class Waypoint : MonoBehaviour
    {
        // 입력 방향 설정
        public enum MoveDirection
        {
            Right,
            Left
        }

        [Header("#Map Setting")] private Transform character;
        [Tooltip("웨이 포인트의 전진 방향을 설정해주세요.")] public MoveDirection moveDirSet; // 웨이포인트 이동방향 세팅용, 코드에서 건들리지 마세요
        internal MoveDirection movingDir; // 이전에 이동 입력한 방향
        internal MoveDirection outMoveDir; // 체크 포인트에서 밖으로 나갈때 오른쪽으로 가는지 왼쪽으로 가는지
        private bool isIn;

        [Header("#Waypoint Setting")] [Tooltip("포인트 체크 가능 거리 설정입니다.")]
        public float checkingDist = 0.7f; // check 가능한 거리

        [Tooltip("체크 포인트의 인덱스입니다.")] public int currentIndex; // 현재 체크 포인트 인덱스
        [Tooltip("웨이 포인트들의 리스트입니다.")] public List<Transform> waypoints; // waypoint 리스트
        private Transform nearestPoint; // 최단 포인트

        public Transform checkedWaypoint; //check된 포인트

        // Right인 경우 fwd으로 Left인 경우 bwd으로 이동

        private void Start()
        {
            if (Application.isPlaying)
            {
                SelectCharacter();
            }
        }

        void Update()
        {
            EditSettingUpdate();

            if (Application.isPlaying)
            {
                PlaySettingUpdate();
            }
        }

        #region 에디트, 맵데이터 세팅

        private void EditSettingUpdate()
        {
            UpdateWaypoints();
        }

        // waypoint 리스트 업데이트
        private void UpdateWaypoints()
        {
            if (waypoints.Count != transform.childCount)
            {
                waypoints = new List<Transform>();
                for (int i = 0; i < transform.childCount; i++)
                {
                    waypoints.Add(transform.GetChild(i)); // 리스트 추가                
                }

                waypoints.Sort(delegate(Transform a, Transform b)
                {
                    if (String.Compare(a.name, b.name, StringComparison.Ordinal) > 0)
                    {
                        return 1;
                    }
                    else
                    {
                        return -1;
                    }
                }); // 이름순으로 정렬
                checkedWaypoint = waypoints[0];
            }
        }

        #endregion

        #region 게임 플레이 세팅 업데이트

        private void PlaySettingUpdate()
        {
            WaypointCheck();
            JoystickInput();
        }

        // waypoint를 따라 움직일 캐릭터 설정
        private void SelectCharacter()
        {
            character = DataController.Instance.GetCharacter(Character.Main).transform;
            DataController.Instance.CurrentMap.isJoystickInputUse = false;
        }

        // 캐릭터가 waypoint를 지나가면 체크
        private void WaypointCheck()
        {
            if (waypoints.Count == 0)
            {
                return;
            }
        
            if (waypoints.Count == 1)
            {
                nearestPoint = waypoints[0];
                checkedWaypoint = waypoints[0];
                return;
            }

            if (checkedWaypoint == null)
            {
                checkedWaypoint = waypoints[0]; // 캐릭터와 waypoint의 거리가 check 가능한게 없으면 1번째 포인트를 check해줌
            }

            currentIndex = waypoints.FindIndex(item => item == checkedWaypoint); // 현재 check된 포인트의 인덱스 반환
            nearestPoint = waypoints[currentIndex];
            float minDist =
                DistanceIgnoreY(character.position, waypoints[currentIndex].position); // 현재 체크 포인트의 거리를 계산후 최단거리 설정

            float frontDis, backDis;
            if (currentIndex == 0)
            {
                frontDis = DistanceIgnoreY(character.position, waypoints[currentIndex + 1].position);
                backDis = float.MaxValue;

            }
            else if (currentIndex == waypoints.Count - 1)
            {
                frontDis = float.MaxValue;
                backDis = DistanceIgnoreY(character.position, waypoints[currentIndex - 1].position);
            }
            else
            {
                frontDis = DistanceIgnoreY(character.position, waypoints[currentIndex + 1].position);
                backDis = DistanceIgnoreY(character.position, waypoints[currentIndex - 1].position);

            }

            if (backDis <= minDist)
            {
                nearestPoint = waypoints[currentIndex - 1];
                minDist = backDis;
            }

            if (frontDis <= minDist)
            {
                nearestPoint = waypoints[currentIndex + 1];
                minDist = frontDis;
            }


            if (DistanceIgnoreY(character.position, nearestPoint.position) <= checkingDist)
            {
                checkedWaypoint = nearestPoint; // 캐릭터와 waypoint의 거리가 check 가능한 거리가 되면 해당 포인트를 check해줌   
            }
        }

        public float DistanceIgnoreY(Vector3 vec1, Vector3 vec2)
        {
            vec1.y = 0;
            vec2.y = 0;
            return Vector3.Distance(vec1, vec2);
        }

        public Vector3 GetMiddlePoint()
        {
            SetBackFrontIndex(out var fwdIndex, out var bwdIndex);
            return ((waypoints[fwdIndex].position - waypoints[currentIndex].position).normalized * checkingDist +
                    (waypoints[bwdIndex].position - waypoints[currentIndex].position).normalized * checkingDist) / 2;
        }

        private void SetBackFrontIndex(out int fwdIndex, out int bwdIndex)
        {
            if (currentIndex + 1 < waypoints.Count) fwdIndex = currentIndex + 1; // 마지막 포인트가 아닐 경우 전진 방향 설정
            else fwdIndex = currentIndex; // 마지막 포인트일 경우 마지막 포인트로 향하도록 전진 방향 설정

            if (currentIndex > 0) bwdIndex = currentIndex - 1; // 마지막 포인트가 아닐 경우 전진 방향 설정
            else bwdIndex = currentIndex; // 마지막 포인트일 경우 마지막 포인트로 향하도록 전진 방향 설정
        }

        // 시작 경로가 오른쪽인 경우와 왼쪽인 경우
        // 입력 방향 세팅
        private void JoystickInput()
        {
            float joystickInput = JoystickController.instance.joystick.Horizontal; // 한 방향 입력은 수평값만 받음

            if (Mathf.Approximately(joystickInput, 0f))
            {
                JoystickController.instance.inputDegree = 0f; // 조정된 입력 방향으로 크기 계산
                return;
            }

            if (JoystickController.instance.joystick.gameObject.activeSelf && waypoints.Count > 0 &&
                DataController.Instance.GetCharacter(Character.Main).IsMove)
            {
                int fwdIndex, bwdIndex;
                SetBackFrontIndex(out fwdIndex, out bwdIndex);

                // Index 설정
                float charDistance = DistanceIgnoreY(character.position, checkedWaypoint.position);
                if (charDistance <= checkingDist)
                {
                    //안에 있을때는 바로 현재 기준 앞 뒤 인덱스로 이동
                    isIn = true;
                }
                else
                {
                    if (isIn)
                    {
                        isIn = false;
                        // 안에서 밖으로 나갈 때 방향 
                        outMoveDir = movingDir;
                    }

                    if (outMoveDir == moveDirSet) // 이동 방향과 입력 방향이 같을때
                    {
                        bwdIndex = currentIndex;
                    }
                    else // 이동 방향과 입력 방향이 다를때
                    {
                        fwdIndex = currentIndex;
                    }
                }

                Vector3 moveDir;

                // Move 세팅
                movingDir = joystickInput > 0 ? MoveDirection.Right : joystickInput < 0 ? MoveDirection.Left : movingDir;

                if (movingDir == moveDirSet)
                {
                    if (isIn)
                    {
                        moveDir = (waypoints[fwdIndex].position - waypoints[currentIndex].position).normalized *
                                  checkingDist -
                                  (waypoints[currentIndex].position - character.position);
                    }
                    else
                    {
                        moveDir = waypoints[fwdIndex].position - character.transform.position;
                    }
                }
                else
                {
                    if (isIn)
                    {
                        moveDir = (waypoints[bwdIndex].position - waypoints[currentIndex].position).normalized *
                                  checkingDist -
                                  (waypoints[currentIndex].position - character.position);
                    }
                    else
                    {
                        moveDir = waypoints[bwdIndex].position - character.transform.position;
                    }
                }

                Quaternion camRotation = Quaternion.Euler(0, -Camera.main.transform.rotation.eulerAngles.y, 0);
                moveDir = camRotation * moveDir;

                Vector2 changedDir = new Vector2(moveDir.x, moveDir.z).normalized;

                JoystickController.instance.inputJump =
                    JoystickController.instance.joystick.Vertical > 0.5f; // 수직 입력이 일정 수치 이상 올라가면 점프 판정
                JoystickController.instance.inputDirection = changedDir; // 조정된 입력 방향 설정
                JoystickController.instance.inputDegree = Mathf.Abs(joystickInput); // 조정된 입력 방향으로 크기 계산

                //애니메이션 세팅 해줘야 되지 않나???
            }
        }

        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (checkedWaypoint == waypoints[i])
                {
                    Gizmos.color = Color.green * 0.7f;
                    Gizmos.DrawSphere(waypoints[i].position, 0.5f);
                    if (i == 0) Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                    else if (i == waypoints.Count - 1) Gizmos.DrawLine(waypoints[i - 1].position, waypoints[i].position);
                    else
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                        Gizmos.DrawLine(waypoints[i - 1].position, waypoints[i].position);
                    }
                }
                else
                {
                    Gizmos.color = Color.red * 0.7f;
                    Gizmos.DrawSphere(waypoints[i].position, 0.5f);
                    if (i != waypoints.Count - 1) Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }

            if (nearestPoint != null)
            {
                Gizmos.color = Color.blue * 0.7f;
                Gizmos.DrawWireSphere(nearestPoint.position, 0.5f);
            }
        }
#endif
    }
}

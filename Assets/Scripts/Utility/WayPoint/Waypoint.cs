using System;
using System.Collections.Generic;
using UnityEngine;
using Utility.Core;
using static Data.CustomEnum;

namespace Utility.WayPoint
{
    public class Waypoint : MonoBehaviour
    {
        public enum MoveDirection
        {
            Right,
            Left
        }

        [Header("#Map Setting")]
        [Tooltip("웨이 포인트의 전진 방향을 설정해주세요.")] public MoveDirection moveDirSet; // 웨이포인트 이동방향 세팅용, 코드에서 건들리지 마세요
        internal MoveDirection MovingDir; // 이전에 이동 입력한 방향

        [Header("#Waypoint Setting")] [Tooltip("포인트 체크 가능 거리 설정입니다.")]
        public float checkingDist = 0.7f; // check 가능한 거리

        public List<Transform> waypoints;

        [NonSerialized] public int CurrentIndex;
        
        private Transform checkedWaypoint;
        private Transform nearestPoint;
        private MoveDirection outMoveDir; // 체크 포인트에서 밖으로 나갈때 오른쪽으로 가는지 왼쪽으로 가는지
        
        private bool isIn;

        private void Start()
        {
            DataController.Instance.CurrentMap.isJoystickInputUse = false;
        }

        private void Update()
        {
            WaypointCheck();
            JoystickInput();
        }

        #region 게임 플레이 세팅 업데이트

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

            var mainCharacter = DataController.Instance.GetCharacter(Character.Main);
            CurrentIndex = waypoints.FindIndex(item => item == checkedWaypoint); // 현재 check된 포인트의 인덱스 반환
            nearestPoint = waypoints[CurrentIndex];
            float minDist =
                DistanceIgnoreY(mainCharacter.transform.position, waypoints[CurrentIndex].position); // 현재 체크 포인트의 거리를 계산후 최단거리 설정

            float frontDis, backDis;
            if (CurrentIndex == 0)
            {
                frontDis = DistanceIgnoreY(mainCharacter.transform.position, waypoints[CurrentIndex + 1].position);
                backDis = float.MaxValue;

            }
            else if (CurrentIndex == waypoints.Count - 1)
            {
                frontDis = float.MaxValue;
                backDis = DistanceIgnoreY(mainCharacter.transform.position, waypoints[CurrentIndex - 1].position);
            }
            else
            {
                frontDis = DistanceIgnoreY(mainCharacter.transform.position, waypoints[CurrentIndex + 1].position);
                backDis = DistanceIgnoreY(mainCharacter.transform.position, waypoints[CurrentIndex - 1].position);

            }

            if (backDis <= minDist)
            {
                nearestPoint = waypoints[CurrentIndex - 1];
                minDist = backDis;
            }

            if (frontDis <= minDist)
            {
                nearestPoint = waypoints[CurrentIndex + 1];
                minDist = frontDis;
            }


            if (DistanceIgnoreY(mainCharacter.transform.position, nearestPoint.position) <= checkingDist)
            {
                checkedWaypoint = nearestPoint; // 캐릭터와 waypoint의 거리가 check 가능한 거리가 되면 해당 포인트를 check해줌   
            }
        }

        public static float DistanceIgnoreY(Vector3 vec1, Vector3 vec2)
        {
            vec1.y = 0;
            vec2.y = 0;
            return Vector3.Distance(vec1, vec2);
        }

        public Vector3 GetMiddlePoint()
        {
            SetBackFrontIndex(out var fwdIndex, out var bwdIndex);
            return waypoints[CurrentIndex].position + 
                   ((waypoints[fwdIndex].position - waypoints[CurrentIndex].position).normalized +
                    (waypoints[bwdIndex].position - waypoints[CurrentIndex].position).normalized) * checkingDist / 2;
        }

        private void SetBackFrontIndex(out int fwdIndex, out int bwdIndex)
        {
            if (CurrentIndex + 1 < waypoints.Count)
            {
                fwdIndex = CurrentIndex + 1; // 마지막 포인트가 아닐 경우 전진 방향 설정
            }
            else
            {
                fwdIndex = CurrentIndex; // 마지막 포인트일 경우 마지막 포인트로 향하도록 전진 방향 설정
            }

            if (CurrentIndex > 0)
            {
                bwdIndex = CurrentIndex - 1; // 마지막 포인트가 아닐 경우 전진 방향 설정
            }
            else
            {
                bwdIndex = CurrentIndex; // 마지막 포인트일 경우 마지막 포인트로 향하도록 전진 방향 설정
            }
        }

        // 시작 경로가 오른쪽인 경우와 왼쪽인 경우
        // 입력 방향 세팅
        private void JoystickInput()
        {
            var mainCharacter = DataController.Instance.GetCharacter(Character.Main);
            var joystickInput = JoystickController.Instance.Joystick.Horizontal;

            if (Mathf.Approximately(joystickInput, 0f))
            {
                JoystickController.Instance.inputDegree = 0f;
                return;
            }

            if (JoystickController.Instance.Joystick.gameObject.activeSelf && waypoints.Count > 0 &&
                DataController.Instance.GetCharacter(Character.Main).IsMove)
            {
                SetBackFrontIndex(out var fwdIndex, out var bwdIndex);

                // Index 설정
                var characterDistance = DistanceIgnoreY(mainCharacter.transform.position, checkedWaypoint.position);
                if (characterDistance <= checkingDist)
                {
                    //안에 있을때는 바로 현재 기준 앞 뒤 인덱스로 이동
                    isIn = true;
                }
                else
                {
                    if (isIn)
                    {
                        // 안에서 밖으로 나갈 때 방향 
                        isIn = false;
                        outMoveDir = MovingDir;
                    }

                    if (outMoveDir == moveDirSet) // 이동 방향과 입력 방향이 같을때
                    {
                        bwdIndex = CurrentIndex;
                    }
                    else // 이동 방향과 입력 방향이 다를때
                    {
                        fwdIndex = CurrentIndex;
                    }
                }

                Vector3 moveDir;

                // Move 세팅
                MovingDir = joystickInput > 0 ? MoveDirection.Right : joystickInput < 0 ? MoveDirection.Left : MovingDir;

                if (MovingDir == moveDirSet)
                {
                    if (isIn)
                    {
                        moveDir = (waypoints[fwdIndex].position - waypoints[CurrentIndex].position).normalized *
                                  checkingDist -
                                  (waypoints[CurrentIndex].position - mainCharacter.transform.position);
                    }
                    else
                    {
                        moveDir = waypoints[fwdIndex].position - mainCharacter.transform.position;
                    }
                }
                else
                {
                    if (isIn)
                    {
                        moveDir = (waypoints[bwdIndex].position - waypoints[CurrentIndex].position).normalized *
                                  checkingDist -
                                  (waypoints[CurrentIndex].position - mainCharacter.transform.position);
                    }
                    else
                    {
                        moveDir = waypoints[bwdIndex].position - mainCharacter.transform.position;
                    }
                }

                var camRotation = Quaternion.Euler(0, -Camera.main.transform.rotation.eulerAngles.y, 0);
                moveDir = camRotation * moveDir;

                var changedDir = new Vector2(moveDir.x, moveDir.z).normalized;

                JoystickController.Instance.InputJump =
                    JoystickController.Instance.Joystick.Vertical > 0.5f; // 수직 입력이 일정 수치 이상 올라가면 점프 판정
                JoystickController.Instance.inputDirection = changedDir; // 조정된 입력 방향 설정
                JoystickController.Instance.inputDegree = Mathf.Abs(joystickInput); // 조정된 입력 방향으로 크기 계산

                //애니메이션 세팅 해줘야 되지 않나???
            }
        }

        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            for(var i = 0; i < waypoints.Count; i++)
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

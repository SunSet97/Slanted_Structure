using System;
using System.Collections.Generic;
using UnityEngine;
using Utility.Core;
using static Data.CustomEnum;

namespace Utility.WayPoint
{
    [Serializable]
    public class WaypointEdge
    {
        public Vector3 frontWaypointPos;
        public Vector3 backWaypointPos;

        public int frontIndex;
        public int backIndex;

        public int GetCloseVertexIndex(Vector3 mainCharacterPos)
        {
            return Waypoint.DistanceIgnoreY(frontWaypointPos, mainCharacterPos) <
                   Waypoint.DistanceIgnoreY(backWaypointPos, mainCharacterPos)
                ? frontIndex
                : backIndex;
        }
    }

    public class Waypoint : MonoBehaviour
    {
        private enum MoveDirection
        {
            Right,
            Left
        }

        public List<Transform> waypoints;

        [Tooltip("포인트 체크 가능 거리 설정입니다.")] public float checkingDist = 0.7f;

        [SerializeField] private int currentEdgeIndex;

        [Header("디버그용")] [SerializeField] private WaypointEdge[] waypointEdges;

        private MoveDirection moveDirSet;
        private bool isInVertex;
        private MoveDirection movedDir;

        private int originalEdgeIndex;
        private void Start()
        {
            originalEdgeIndex = currentEdgeIndex;
            Init();
        }
        
        public void Init()
        {
            isInVertex = false;
            moveDirSet = DataController.Instance.CurrentMap.rightIsForward ? MoveDirection.Right : MoveDirection.Left;
            currentEdgeIndex = originalEdgeIndex;
            
            waypointEdges = new WaypointEdge[waypoints.Count - 1];
            for (var index = 0; index < waypoints.Count - 1; index++)
            {
                waypointEdges[index] = new WaypointEdge
                {
                    frontIndex = index + 1,
                    backIndex = index,
                    frontWaypointPos = waypoints[index + 1].position,
                    backWaypointPos = waypoints[index].position
                };
            }
        }

        public void JoystickUpdate()
        {
            var mainCharacter = DataController.Instance.GetCharacter(Character.Main);
            var joystickInput = JoystickController.Instance.Joystick.Horizontal;

            if (waypoints.Count <= 0)
            {
                return;
            }

            var closeIndex = waypointEdges[currentEdgeIndex].GetCloseVertexIndex(mainCharacter.transform.position);

            var characterDistance = DistanceIgnoreY(mainCharacter.transform.position, waypoints[closeIndex].position);
            if (characterDistance <= checkingDist)
            {
                isInVertex = true;
            }
            else
            {
                // 안에서 밖으로 나갈 때
                if (isInVertex)
                {
                    isInVertex = false;
                    if (closeIndex == waypointEdges[currentEdgeIndex].frontIndex && moveDirSet == movedDir)
                    {
                        currentEdgeIndex = Mathf.Clamp(currentEdgeIndex + 1, 0, waypointEdges.Length - 1);
                    }
                    else if (closeIndex == waypointEdges[currentEdgeIndex].backIndex)
                    {
                        currentEdgeIndex = Mathf.Clamp(currentEdgeIndex - 1, 0, waypointEdges.Length - 1);
                    }
                }
            }

            if (joystickInput > 0)
            {
                movedDir = MoveDirection.Right;
            }
            else if (joystickInput < 0)
            {
                movedDir = MoveDirection.Left;
            }

            int targetIndex;
            if (movedDir == moveDirSet)
            {
                if (closeIndex == waypointEdges[currentEdgeIndex].frontIndex && isInVertex)
                {
                    // Debug.LogWarning("다음 인덱스로 정면");
                    targetIndex = waypointEdges[Mathf.Clamp(currentEdgeIndex + 1, 0, waypointEdges.Length - 1)]
                        .frontIndex;
                }
                else
                {
                    targetIndex = waypointEdges[currentEdgeIndex].frontIndex;
                }
            }
            else
            {
                if (closeIndex == waypointEdges[currentEdgeIndex].backIndex && isInVertex)
                {
                    // Debug.LogWarning("뒤에 인덱스로 후면");
                    targetIndex = waypointEdges[Mathf.Clamp(currentEdgeIndex - 1, 0, waypointEdges.Length - 1)]
                        .backIndex;
                }
                else
                {
                    targetIndex = waypointEdges[currentEdgeIndex].backIndex;
                }
            }

            // Debug.Log("노리는 Index: " + targetIndex);

            Vector3 targetPos;
            if (isInVertex)
            {
                if ((waypointEdges[currentEdgeIndex].backIndex == targetIndex && targetIndex == 0) || (waypointEdges[currentEdgeIndex].frontIndex == targetIndex && targetIndex == waypoints.Count - 1))
                {
                    // Debug.Log($"멈춤 후진 멈춤: {movedDir != moveDirSet && targetIndex == 0}, 정면 멈춤: {movedDir == moveDirSet && targetIndex == waypoints.Count - 1}");
                    JoystickController.Instance.inputDegree = 0f;
                    JoystickController.Instance.inputDirection = Vector2.zero;
                    return;
                }

                var nearIndex = waypointEdges[currentEdgeIndex].GetCloseVertexIndex(mainCharacter.transform.position);
                targetPos =
                    (waypoints[targetIndex].position - waypoints[nearIndex].position).normalized * checkingDist +
                    waypoints[nearIndex].position;
            }
            else
            {
                targetPos = waypoints[targetIndex].position;
            }

            var moveDir = targetPos - mainCharacter.transform.position;

            var camRotation = Quaternion.Euler(0, -DataController.Instance.Cam.transform.rotation.eulerAngles.y, 0);
            moveDir = camRotation * moveDir;
            var changedDir = new Vector2(moveDir.x, moveDir.z).normalized;

            JoystickController.Instance.inputDirection = changedDir;
        }

        public static float DistanceIgnoreY(Vector3 vec1, Vector3 vec2)
        {
            vec1.y = 0;
            vec2.y = 0;
            return Vector3.Distance(vec1, vec2);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            for (var i = 0; i < waypoints.Count; i++)
            {
                Gizmos.color = Color.blue * 0.7f;
                Gizmos.DrawSphere(waypoints[i].position, 0.5f);

                if (i != waypoints.Count - 1)
                {
                    Gizmos.color = Color.red * 0.7f;
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }
        }
#endif
    }
}

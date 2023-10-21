using System;
using System.Linq;
using Data;
using UnityEditor;
using UnityEngine;
using Utility.Character;
using Utility.Core;

namespace Utility.WayPoint
{
#if UNITY_EDITOR
    [CustomEditor(typeof(CameraTracker))]
    public class CameraTrackerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var t = (CameraTracker)target;
            if (GUILayout.Button("코너 세팅 버튼"))
            {
                t.SetCorner();
            }
        }
    }
#endif

    public class CameraTracker : MonoBehaviour
    {
        public Waypoint waypoint;
        public float cornerRadius;

        private float camRotDir;

        [SerializeField] public CornerCameraSetting[] cornerCameraSettings;

        [Serializable]
        public struct CornerCameraSetting
        {
            [Header("코너 Waypoint 개체를 넣으세요")] public Transform corner;

            public Transform front;

            public Transform back;

            //왼쪽이 이동방향이면 오른쪽, 왼쪽 순으로
            //오른쪽이 이동방향이면 왼쪽, 오른쪽 순으로
            [Header("이동방향 순서대로 넣으세요")] public CamInfo[] camSettings;
        }

        private void Update()
        {
            if (CheckCorner(out var cornerIndex))
            {
                var character = DataController.Instance.GetCharacter(CharacterType.Main).transform;

                var corner = cornerCameraSettings[cornerIndex];
                // 앞 == 1, 뒤 == -1, 중간 == 0 
                camRotDir = GetDir(corner);

                float ratio = Waypoint.DistanceIgnoreY(character.position, corner.corner.position) / cornerRadius;

                // -1 ~ 1 -> 0 ~ 2 -> 0 ~ 1    0.5는 가운데  뒤인 경우 0 ~ 0.5, 앞인 경우 0.5 ~ 1
                var t = (camRotDir * ratio + 1) * 0.5f;

                // Debug.Log(t);
                var camDis = Vector3.Lerp(corner.camSettings[0].camDis, corner.camSettings[1].camDis, t);
                camDis = Vector3.Lerp(DataController.Instance.camOffsetInfo.camDis, camDis, 0.04f);
                var camRot = Vector3.Lerp(corner.camSettings[0].camRot, corner.camSettings[1].camRot, t);
                camRot = Vector3.Lerp(DataController.Instance.camOffsetInfo.camRot, camRot, 0.04f);
                DataController.Instance.camOffsetInfo.camDis = camDis;
                DataController.Instance.camOffsetInfo.camRot = camRot;
            }
            else
            {
                var corner = cornerCameraSettings[cornerIndex];
                camRotDir = GetDir(corner);

                var camDis = DataController.Instance.camOffsetInfo.camDis;
                var camRot = DataController.Instance.camOffsetInfo.camRot;
                if (Mathf.Approximately(camRotDir, 1f))
                {
                    camDis = Vector3.Lerp(camDis, corner.camSettings[1].camDis,0.05f);
                    camRot = Vector3.Lerp(camRot, corner.camSettings[1].camRot,0.05f);
                }
                else if (Mathf.Approximately(camRotDir, -1f))
                {
                    camDis = Vector3.Lerp(camDis, corner.camSettings[0].camDis,0.05f);
                    camRot = Vector3.Lerp(camRot, corner.camSettings[0].camRot,0.05f);
                }

                DataController.Instance.camOffsetInfo.camDis = camDis;
                DataController.Instance.camOffsetInfo.camRot = camRot;
            }
        }


        private static int GetDir(CornerCameraSetting corner)
        {
            var character = DataController.Instance.GetCharacter(CharacterType.Main).transform;
            var frontDir = corner.front.position - corner.corner.position;
            var backDir = corner.back.position - corner.corner.position;
            var charDir = corner.corner.position - character.position;

            var frontAngle = Vector3.Angle(charDir, frontDir);
            var backAngle = Vector3.Angle(charDir, backDir);
            if (frontAngle > backAngle)
            {
                return 1;
            }

            if (frontAngle < backAngle)
            {
                return -1;
            }
            return 0;
        }

        private bool CheckCorner(out int cornerIndex)
        {
            var character = DataController.Instance.GetCharacter(CharacterType.Main).transform;
            var characterVec = character.position;
            characterVec.y = 0;
            Vector3 cornerVec;

            var closeCorner = cornerCameraSettings.OrderBy(item =>
            {
                cornerVec = item.corner.position;
                cornerVec.y = 0f;
                return Vector3.Distance(characterVec, cornerVec);
            }).First();

            cornerIndex = Array.FindIndex(cornerCameraSettings, item => item.corner == closeCorner.corner);

            cornerVec = closeCorner.corner.position;
            cornerVec.y = 0f;

            return Vector3.Distance(characterVec, cornerVec) <= cornerRadius;
        }

        #if UNITY_EDITOR
        public void SetCorner()
        {
            for(var i = 0; i < cornerCameraSettings.Length; i++)
            {
                var index = waypoint.waypoints.FindIndex(item => item.Equals(cornerCameraSettings[i].corner));
                var front = index + 1;
                var back = index - 1;
                var cornerPos = waypoint.waypoints[index].position;
                var frontDir = (waypoint.waypoints[front].position - cornerPos).normalized;
                var backDir = (waypoint.waypoints[back].position - cornerPos).normalized;
                GameObject temp;
                if (cornerCameraSettings[i].front == null)
                {
                    var tempTrans = waypoint.waypoints[index].Find(cornerCameraSettings[i].corner.name + " front");
                    if (!tempTrans)
                    {
                        temp = new GameObject(waypoint.waypoints[index].gameObject.name + " front");
                    }
                    else
                    {
                        temp = tempTrans.gameObject;
                    }
                    cornerCameraSettings[i].front = Instantiate(temp).transform;
                    cornerCameraSettings[i].front.parent = waypoint.waypoints[index];
                    cornerCameraSettings[i].front.position = cornerPos + frontDir;
                }

                if (cornerCameraSettings[i].back == null)
                {
                    var tempTrans = waypoint.waypoints[index].Find(cornerCameraSettings[i].corner.name + " back");
                    if (!tempTrans)
                    {
                        temp = new GameObject(waypoint.waypoints[index].gameObject.name + " back");
                    }
                    else
                    {
                        temp = tempTrans.gameObject;
                    }
                    cornerCameraSettings[i].back = Instantiate(temp).transform;
                    cornerCameraSettings[i].back.parent = waypoint.waypoints[index];
                    cornerCameraSettings[i].back.position = cornerPos + backDir;
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow - Color.black * 0.4f;
            foreach (var t in cornerCameraSettings)
            {
                Gizmos.DrawSphere(t.corner.position, cornerRadius);
            }
        }
        #endif
    }
}
using UnityEngine;
using System;
using UnityEditor;
using static Data.CustomEnum;

[CustomEditor(typeof(CameraMovingManager))]
public class CamerMovingManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CameraMovingManager t = (CameraMovingManager) target;
        if (GUILayout.Button("코너 세팅 버튼"))
        {
            t.SetCorner();
        }
        
    }
}
public class CameraMovingManager : MonoBehaviour
{
    public Waypoint waypoint;           // 코너 웨이포인트
    public float cornerRadius;              // 코너 범위
    
    private Transform character;   
    private float camRotDir;    // 카메라 회전 방향 인덱스

    [SerializeField]
    public CornerCameraSetting[] cornerCameraSettings;
    
    [Serializable]
    public struct CornerCameraSetting
    {
        [Header("코너 Waypoint 개체를 넣으세요")]
        public Transform corner;
        
        public Transform front;
        public Transform back;
        //왼쪽이 이동방향이면 오른쪽, 왼쪽 순으로
        //오른쪽이 이동방향이면 왼쪽, 오른쪽 순으로
        [Header("웨이포인트처럼 뒤, 앞 순서대로 넣으세요")]
        public CamInfo[] camSettings;
    }
    // 카메라 세팅 구조체
    [Serializable]
    public class CamInfo {
        public Vector3 camDis;
        public Vector3 camRot;
    }

    private void Start()
    {
        character = DataController.instance.GetCharacter(Character.Main)
            .transform;
    }

    void Update()
    {
        if (CheckDistance(out int cornerIndex))
        {
            CornerCameraSetting corner = cornerCameraSettings[cornerIndex];
            //현재 위치가 앞(1)인지 뒤(-1)인지
            camRotDir = GetDir(corner);
            // 카메라가 바라보는 방향과 스핏의 위치에 따라서 자연스럽게 회전하도록 계산
            //앞 뒤 비율 계산
            // float frontDistance = Vector3.Distance(character.position, corner.front.position);
            // float backDistance = Vector3.Distance(character.position, corner.back.position);
            // float t  = backDistance / (frontDistance + backDistance);
            // Debug.Log(frontDistance  + "  " + backDistance);
            float distance;
            if (waypoint.DistanceIgnoreY(corner.corner.position, character.position) < waypoint.checkingDist)
            {
                distance = waypoint.DistanceIgnoreY(waypoint.GetMiddlePoint(), character.position);
            }
            else
            {
                distance = waypoint.DistanceIgnoreY(character.position, corner.corner.position);
            }

            // -1 ~ 1 -> 0 ~ 2 -> 0 ~ 1    0.5는 가운데  뒤인 경우 0 ~ 0.5, 앞인 경우 0.5 ~ 1
            float t = (camRotDir * (distance / cornerRadius) + 1) * 0.5f;
            //Debug.Log(waypoint.movingDir + " " + waypoint.outMoveDir + " " + waypoint.moveDirSet);
            Debug.Log(t);
            // t(0 ~ 1)에 따라 Vector3.Lerp(camSetting[0], camSetting[1], t)
            Vector3 camDis = Vector3.Lerp(corner.camSettings[0].camDis, corner.camSettings[1].camDis, t);
            camDis = Vector3.Lerp(DataController.instance.camDis, camDis, 0.05f);
            Vector3 camRot = Vector3.Lerp(corner.camSettings[0].camRot, corner.camSettings[1].camRot, t);
            camRot = Vector3.Lerp(DataController.instance.camRot, camRot, 0.05f);
            DataController.instance.camDis = camDis;
            DataController.instance.camRot = camRot;
        }
        else
        {
            CornerCameraSetting corner = cornerCameraSettings[cornerIndex];
            camRotDir = GetDir(corner);

            Vector3 camDis;
            Vector3 camRot;
            if (camRotDir == 1)
            {
                camDis = Vector3.Lerp(DataController.instance.camDis, corner.camSettings[1].camDis,
                    0.05f);
                camRot = Vector3.Lerp(DataController.instance.camRot, corner.camSettings[1].camRot,
                    0.05f);
            }
            else
            {
                camDis = Vector3.Lerp(DataController.instance.camDis, corner.camSettings[0].camDis,
                    0.05f);
                camRot = Vector3.Lerp(DataController.instance.camRot, corner.camSettings[0].camRot,
                    0.05f);
            }

            DataController.instance.camDis = camDis;
            DataController.instance.camRot = camRot;
            Debug.Log(camDis);
            Debug.Log(corner.corner);
        }
    }


    int GetDir(CornerCameraSetting corner)
    {
        var frontDir = corner.front.position - corner.corner.position;
        var backDir = corner.back.position - corner.corner.position;
        var charDir = corner.corner.position - character.position;

        var frontAngle = Vector3.Angle(charDir, frontDir);
        var backAngle = Vector3.Angle(charDir, backDir);
        if (frontAngle > backAngle)
        {
            return 1;
        }
        return -1;
    }

    private bool CheckDistance(out int index)
    {
        Vector3 charVec = character.position;
        charVec.y = 0;
        float min = float.MaxValue;
        index = default;
        for(int i = 0; i < cornerCameraSettings.Length; i++)
        {
            Vector3 cornerVec = cornerCameraSettings[i].corner.position;
            cornerVec.y = 0;
            float distance = Vector3.Distance(cornerVec, charVec);
            if (distance <= min)
            {
                min = distance;
                index = i;
                if (distance < cornerRadius)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void SetCorner()
    {
        for (int i = 0; i < cornerCameraSettings.Length; i++)
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
                temp = new GameObject(waypoint.waypoints[index].name + " front");
                cornerCameraSettings[i].front = Instantiate(temp).transform;
                cornerCameraSettings[i].front.parent = waypoint.waypoints[index];
                cornerCameraSettings[i].front.position = cornerPos + frontDir;
            }

            if (cornerCameraSettings[i].back == null)
            {
                temp = new GameObject(waypoint.waypoints[index].name + " back");
                cornerCameraSettings[i].back = Instantiate(temp).transform;
                cornerCameraSettings[i].back.parent = waypoint.waypoints[index];
                cornerCameraSettings[i].back.position = cornerPos + backDir;
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        // 코너 판단영역 기즈모
        Gizmos.color = Color.yellow - Color.black * 0.4f;
        foreach (var t in cornerCameraSettings)
        {
            Gizmos.DrawSphere(t.corner.position, cornerRadius);    
        }
    }
}

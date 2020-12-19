using UnityEngine;
using System;

public class SpeatTutorialSewerManager : MonoBehaviour
{
    public Waypoint waypoint;
    public Transform cornerPosition;
    public float cornerRadius;

    // 카메라 세팅 구조체
    [Serializable]
    public struct CamSetting {
        public Vector3 camDis; public Vector3 camRot;
        public CamSetting(Vector3 camDis, Vector3 camRot) { this.camDis = camDis; this.camRot = camRot; }
    }
    public CamSetting[] camSettings; // 카메라 세팅들
    private float camRotDir = 1; // 카메라 회전 방향 인덱스

    void Update()
    {
        // 코너에 스핏 있는지 판정
        RaycastHit[] hits = Physics.SphereCastAll(cornerPosition.position, cornerRadius, Vector3.up, 0f);
        bool isCorner = false;
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject != null && DataController.instance_DataController.currentChar != null)
                isCorner = hit.collider.name == DataController.instance_DataController.currentChar.name ? true : false;
            else
                isCorner = false;
        }
        // 스핏의 위치에 따라서 카메라 세팅값 결정
        int index = 0;
        if (isCorner)
        {
            index = 1; camRotDir = waypoint.idx != 1 ? 1 : waypoint.moveTo == Waypoint.MoveDirection.Right ? 1 : -1;
            // 카메라가 바라보는 방향과 스핏의 위치에 따라서 자연스럽게 회전하도록 계산
            float distance = Vector3.Distance(DataController.instance_DataController.currentChar.transform.position, cornerPosition.position);
            float theta = 45 * (1 + camRotDir * (distance / cornerRadius));
            camSettings[index] = new CamSetting(new Vector3(-8 * Mathf.Cos(theta * Mathf.Deg2Rad), 2, -8 * Mathf.Sin(theta * Mathf.Deg2Rad)), new Vector3(10, 90 - theta, 0));
        }
        else if ((waypoint.idx == 1 && waypoint.moveTo == Waypoint.MoveDirection.Right) || waypoint.idx == 0)
        {
            index = 0; camRotDir = 1;
        }
        else if ((waypoint.idx == 1 && waypoint.moveTo == Waypoint.MoveDirection.Left) || waypoint.idx == 2)
        {
            index = 2; camRotDir = -1;
        }
        // 인덱스에 맞는 카메라 세팅값 적용
        DataController.instance_DataController.camDis = Vector3.Lerp(DataController.instance_DataController.camDis, camSettings[index].camDis, 0.05f);
        DataController.instance_DataController.rot = Vector3.Lerp(DataController.instance_DataController.rot, camSettings[index].camRot, 0.05f);
    }

    private void OnDrawGizmos()
    {
        // 코너 판단영역 기즈모
        Gizmos.color = Color.yellow - Color.black*0.4f;
        Gizmos.DrawSphere(cornerPosition.position, cornerRadius);
    }
}

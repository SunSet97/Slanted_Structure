using UnityEngine;
using System;
using Utility.Core;
using Utility.WayPoint;

public class AlleyInMarketManager : MonoBehaviour
{
    public Waypoint waypoint;
    public Transform AutoMovingBox;

    [Serializable]
    public struct CamSetting
    {
        public Vector3 camDis; public Vector3 camRot;
        public CamSetting(Vector3 camDis, Vector3 camRot) { this.camDis = camDis; this.camRot = camRot; }
    }
    public CamSetting[] camSettings; // 카메라 세팅들
    private float camRotDir = 1; // 카메라 회전 방향 인덱스

    // Update is called once per frame
    void Update()
    {
        int index = 0;

        if ((waypoint.CurrentIndex == 1 && waypoint.MovingDir == Waypoint.MoveDirection.Left) || waypoint.CurrentIndex == 0)
        {
            index = 0; camRotDir = 1;
        }
        else if ((waypoint.CurrentIndex == 2 && waypoint.MovingDir == Waypoint.MoveDirection.Left) || waypoint.CurrentIndex == 1)
        {
            index = 1; camRotDir = 1;
        }
        else if ((waypoint.CurrentIndex == 1 && waypoint.MovingDir == Waypoint.MoveDirection.Right) || waypoint.CurrentIndex == 2 || (waypoint.CurrentIndex == 3 && waypoint.MovingDir == Waypoint.MoveDirection.Left))
        {
            index = 2; camRotDir = -1;
        }
        else if ((waypoint.CurrentIndex == 2 && waypoint.MovingDir == Waypoint.MoveDirection.Right) || waypoint.CurrentIndex == 3 || (waypoint.CurrentIndex == 4 && waypoint.MovingDir == Waypoint.MoveDirection.Left))
        {
            index = 3;
        }
        else if ((waypoint.CurrentIndex == 3 && waypoint.MovingDir == Waypoint.MoveDirection.Right) || waypoint.CurrentIndex == 4)
        {
            index = 4;
        }

        DataController.Instance.camInfo.camDis = Vector3.Lerp(DataController.Instance.camInfo.camDis, camSettings[index].camDis, 0.05f);
        DataController.Instance.camInfo.camRot = Vector3.Lerp(DataController.Instance.camInfo.camRot, camSettings[index].camRot, 0.05f);

    }

}

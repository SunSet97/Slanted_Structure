using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Utility.System;

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

        if ((waypoint.currentIndex == 1 && waypoint.movingDir == Waypoint.MoveDirection.Left) || waypoint.currentIndex == 0)
        {
            index = 0; camRotDir = 1;
        }
        else if ((waypoint.currentIndex == 2 && waypoint.movingDir == Waypoint.MoveDirection.Left) || waypoint.currentIndex == 1)
        {
            index = 1; camRotDir = 1;
        }
        else if ((waypoint.currentIndex == 1 && waypoint.movingDir == Waypoint.MoveDirection.Right) || waypoint.currentIndex == 2 || (waypoint.currentIndex == 3 && waypoint.movingDir == Waypoint.MoveDirection.Left))
        {
            index = 2; camRotDir = -1;
        }
        else if ((waypoint.currentIndex == 2 && waypoint.movingDir == Waypoint.MoveDirection.Right) || waypoint.currentIndex == 3 || (waypoint.currentIndex == 4 && waypoint.movingDir == Waypoint.MoveDirection.Left))
        {
            index = 3;
        }
        else if ((waypoint.currentIndex == 3 && waypoint.movingDir == Waypoint.MoveDirection.Right) || waypoint.currentIndex == 4)
        {
            index = 4;
        }

        DataController.instance.camInfo.camDis = Vector3.Lerp(DataController.instance.camInfo.camDis, camSettings[index].camDis, 0.05f);
        DataController.instance.camInfo.camRot = Vector3.Lerp(DataController.instance.camInfo.camRot, camSettings[index].camRot, 0.05f);

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

        if ((waypoint.idx == 1 && waypoint.moveTo == Waypoint.MoveDirection.Left) || waypoint.idx == 0)
        {
            index = 0; camRotDir = 1;
        }
        else if ((waypoint.idx == 2 && waypoint.moveTo == Waypoint.MoveDirection.Left) || waypoint.idx == 1)
        {
            index = 1; camRotDir = 1;
        }
        else if ((waypoint.idx == 1 && waypoint.moveTo == Waypoint.MoveDirection.Right) || waypoint.idx == 2 || (waypoint.idx == 3 && waypoint.moveTo == Waypoint.MoveDirection.Left))
        {
            index = 2; camRotDir = -1;
        }
        else if ((waypoint.idx == 2 && waypoint.moveTo == Waypoint.MoveDirection.Right) || waypoint.idx == 3 || (waypoint.idx == 4 && waypoint.moveTo == Waypoint.MoveDirection.Left))
        {
            index = 3;
        }
        else if ((waypoint.idx == 3 && waypoint.moveTo == Waypoint.MoveDirection.Right) || waypoint.idx == 4)
        {
            index = 4;
        }

        DataController.instance_DataController.camDis = Vector3.Lerp(DataController.instance_DataController.camDis, camSettings[index].camDis, 0.05f);
        DataController.instance_DataController.rot = Vector3.Lerp(DataController.instance_DataController.rot, camSettings[index].camRot, 0.05f);

    }

}

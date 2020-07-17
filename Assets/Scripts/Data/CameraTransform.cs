using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct cameraSetting
{
    public Vector3 position;
    public Vector3 rotate;
}

public class CameraTransform : MonoBehaviour
{
    public bool isChange;
    public float dis_Z;
    public float dis_X;
    public float charZ;
    public Vector3 rotation;

    //TestCode
    public void changeCam()
    {
        DataController.instance_DataController.camDis_x = 10;
        DataController.instance_DataController.camDis_y = 10;
        DataController.instance_DataController.camDis_z = 0;
    }
    
    public cameraSetting getSettings()
    {
        var returnValue = new cameraSetting();
        returnValue.position = new Vector3(0,0,0);
        returnValue.rotate = rotation;

        return returnValue;
    }
}

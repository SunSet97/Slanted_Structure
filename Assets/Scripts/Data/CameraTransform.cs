using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTransform : MonoBehaviour
{
    public bool isChange;
    public float dis_Z;
    public float dis_X;
    public float charZ;
    public Vector3 rotation;

    public GameObject character;
    public Vector3 _relavitePosition;
    public Vector3 _rotate;
    public bool isLookAt;

    private string camStr;
    
    public CameraTransform()
    {
        var camSettings = new CameraSetting();
        camSettings._relativePosition = _relavitePosition;
        camSettings.rotate = _rotate;
        camSettings.target = character;
        camSettings.isLookAt = isLookAt;

        camStr = CameraController.Instance.addCamSetting(camSettings);
    }
    
    //TestCode
    public void changeCam()
    {
        CameraController.Instance.changeCameraSetting(camStr);
    }
}

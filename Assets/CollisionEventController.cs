using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata;
using UnityEngine;

public class CollisionEventController : MonoBehaviour
{
    private CanvasControl canvasCtrl;
    public void Start()
    {
        canvasCtrl = CanvasControl.instance_CanvasControl;
    }

    public void changeAngle(CameraTransform ct)
    {
        DataController.instance_DataController.camDis_x = ct.dis_X;
        DataController.instance_DataController.camDis_z = ct.dis_Z;
        DataController.instance_DataController.rot = ct.rotation;
    }

    public void colliderNext()
    {
        canvasCtrl.isGoNextStep = true;
        canvasCtrl.progressIndex++;
        canvasCtrl.GoNextStep();
    }
}

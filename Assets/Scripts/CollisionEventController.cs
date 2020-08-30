using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata;
using UnityEngine;

public enum mapType
{
    RauTutorial,
    SpeatTutorial
}

public class CollisionEventController : MonoBehaviour
{
    private CanvasControl canvasCtrl;
    private mapType type;
    public void Start()
    {
        canvasCtrl = CanvasControl.instance_CanvasControl;
    }

    public void setTypeRauTutorial()
    {
        type = mapType.RauTutorial;
    }
    
    public void script(GameObject go)
    {
        var value = go.transform.parent.name + "/" + type.ToString() + "/";
        DataController.instance_DataController.LoadData(value,go.name + ".json");
        canvasCtrl.StartConversation();
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

    public void tutorialCmd()
    {
        canvasCtrl.TutorialCmdCtrl();
    }

    public void setFalseCurrentCollider()
    {
        canvasCtrl.setFalse();
    }
}

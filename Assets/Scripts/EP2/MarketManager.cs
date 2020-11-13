using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketManager : MonoBehaviour
{
    bool completeSetCam = false;
    // Start is called before the first frame update
    void Start()
    {
        // 카메라 조절
        DataController.instance_DataController.camDis = new Vector3(0,10,-5);
        DataController.instance_DataController.rot = new Vector3(45, 0, 0);
    }

    void Update() {
        if (!completeSetCam) {
            DataController.instance_DataController.camDis = new Vector3(0, 10, -5);
            DataController.instance_DataController.rot = new Vector3(45, 0, 0);
            completeSetCam = true;
        }
    }

}

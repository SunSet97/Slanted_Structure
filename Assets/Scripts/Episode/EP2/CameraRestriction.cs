using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Data.CustomEnum;
public class CameraRestriction : MonoBehaviour
{
    Camera_Moving camera_Moving;

    void Start() {
        camera_Moving = DataController.instance.cam.GetComponent<Camera_Moving>();
    }

    private void OnTriggerEnter(Collider other) {

        if (other.name == DataController.instance.GetCharacter(Character.Main).name) {

            if (gameObject.name == "hor" && !camera_Moving.enabled)
            {
                camera_Moving.enabled = true;
            }
            else if (gameObject.name == "ver")
            {

            }


        }

    }

    void fixVer() {

    }

    void fixHor() {

    }

}

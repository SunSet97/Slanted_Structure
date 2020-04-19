using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectAroundPigeon_phw : MonoBehaviour
{
    public bool detect = false;
    public bool successEscape = false;

    void OnTriggerEnger(Collider other) {
        if (other.name == "Rau_phw") {
            detect = true;
        }

        /*if (other.tag == "jump_on_pigeon") {
            successEscape = true;
        }*/
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieBySewage : MonoBehaviour
{
    public Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if(other.name == "Speat")
        {
            other.GetComponent<CharacterManager>().isControlled = false;
            other.GetComponent<Animator>().applyRootMotion = false;
            other.transform.position = respawnPoint.position;
        }
    }
}

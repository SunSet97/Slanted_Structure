using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event_DieBySewage : MonoBehaviour
{
    public Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        //if (other.name == "Speat")
        {
            other.GetComponent<CharacterManager>().PickUpCharacter();
            other.transform.position = respawnPoint.position; // 리스폰 포인트로 이동
            other.GetComponent<CharacterManager>().UseJoystickCharacter();
        }
    }
}

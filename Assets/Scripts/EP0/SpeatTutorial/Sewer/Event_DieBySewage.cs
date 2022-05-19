using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event_DieBySewage : MonoBehaviour
{
    public Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        var character = other.GetComponent<CharacterManager>();
        character.PickUpCharacter();
        other.transform.position = respawnPoint.position; // 리스폰 포인트로 이동
        character.UseJoystickCharacter();
    }
}

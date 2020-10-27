using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event_DieByWater : MonoBehaviour
{
    public Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Rau")
        {
            other.GetComponent<CharacterManager>().PickUpCharacter();
            other.transform.position = respawnPoint.position; // 리스폰 포인트로 이동
            other.GetComponent<CharacterManager>().UseJoystickCharacter();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(respawnPoint.position, 0.5f);
    }
}

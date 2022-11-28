using UnityEngine;

public class Event_DieBySewage : MonoBehaviour
{
    public Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject);
        var character = other.GetComponent<CharacterManager>();
        character.PickUpCharacter();
        other.transform.position = respawnPoint.position; // 리스폰 포인트로 이동
        character.UseJoystickCharacter();
    }
}

using Data;
using UnityEngine;

public class Event_DieByWater : MonoBehaviour
{
    public Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CharacterManager character))
        {
            if (character.who != CustomEnum.Character.Rau) return;
            character.PickUpCharacter();
            character.transform.position = respawnPoint.position; // 리스폰 포인트로 이동
            character.UseJoystickCharacter();
            GetComponentInParent<RauTutorialManager>().isFallInRiver = true;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(respawnPoint.position, 0.5f);
    }
}

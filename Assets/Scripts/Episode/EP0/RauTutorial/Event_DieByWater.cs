using Data;
using UnityEngine;

public class Event_DieByWater : MonoBehaviour
{
    public Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out CharacterManager character) &&
            character != DataController.instance.GetCharacter(CustomEnum.Character.Main))
        {
            return;
        }

        character.PickUpCharacter();
        other.transform.position = respawnPoint.position; // 리스폰 포인트로 이동
        character.UseJoystickCharacter();
        GetComponentInParent<RauTutorialManager>().isFallInRiver = true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(respawnPoint.position, 0.5f);
    }
}

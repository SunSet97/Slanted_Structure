using UnityEngine;
using Utility.Character;
using Utility.Core;
using Utility.WayPoint;

namespace Episode.EP0.SpeatTutorial.Sewer
{
    public class EventDieBySewage : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private Transform respawnPoint;
        [SerializeField] private Waypoint waypoint;
#pragma warning restore 0649
        
        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out CharacterManager character) &&
                character != DataController.Instance.GetCharacter(CharacterType.Main) || !respawnPoint)
            {
                return;
            }

            character.PickUpCharacter();
            other.transform.position = respawnPoint.position;
            character.UseJoystickCharacter();
            waypoint.Init();
        }
    }
}
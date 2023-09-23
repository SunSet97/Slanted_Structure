using Data;
using UnityEngine;
using Utility.Core;
using Utility.WayPoint;

namespace Episode.EP0.SpeatTutorial.Sewer
{
    public class EventDieBySewage : MonoBehaviour
    {
        [SerializeField] private Transform respawnPoint;
        [SerializeField] private Waypoint waypoint;
        
        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out CharacterManager character) &&
                character != DataController.Instance.GetCharacter(CustomEnum.Character.Main) || !respawnPoint)
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
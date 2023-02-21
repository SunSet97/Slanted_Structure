using CommonScript;
using Data;
using UnityEngine;
using Utility.Core;

namespace Episode.EP0.SpeatTutorial.Sewer
{
    public class EventDieBySewage : MonoBehaviour
    {
        public Transform respawnPoint;
        
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
        }
    }
}

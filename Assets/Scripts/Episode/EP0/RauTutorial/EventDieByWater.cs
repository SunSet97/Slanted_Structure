using Data;
using UnityEngine;
using Utility.Core;

namespace Episode.EP0.RauTutorial
{
    public class EventDieByWater : MonoBehaviour
    {
        [SerializeField] private RauTutorialManager rauTutorialManager;
        [SerializeField] private Transform respawnPoint;
        [SerializeField] private TextAsset jsonFile;

        private bool isInteracted;

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
            if (!isInteracted)
            {
                isInteracted = true;
                DialogueController.Instance.StartConversation(jsonFile.text);
                DialogueController.Instance.SetDialogueEndAction(() =>
                {
                    StartCoroutine(rauTutorialManager.FallInRiver());
                });
            }
        }

        private void OnDrawGizmos()
        {
            if (!respawnPoint || !Application.isEditor)
            {
                return;
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(respawnPoint.position, 0.5f);
        }
    }
}
using UnityEngine;
using UnityEngine.Serialization;
using Utility.Core;
using Utility.Property;

namespace Utility.Interaction
{
    public class InteractableCollider : MonoBehaviour
    {
        [SerializeField] private InteractionObject interactionObject;
    
        [FormerlySerializedAs("useExclamationMark")] [Header("#Mark setting")]
        public bool useMark;
        [ConditionalHideInInspector("useMark")]
        [SerializeField] private GameObject markPrefab;
        [ConditionalHideInInspector("useMark")]
        [SerializeField] private Vector2 markOffset;

        private void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
            if (useMark)
            {
                interactionObject.ExclamationMark = Instantiate(markPrefab, DataController.Instance.CurrentMap.ui);
                interactionObject.ExclamationMark.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (useMark)
            {
                Destroy(interactionObject.ExclamationMark);
            }
        }

        private void Update()
        {
            if (interactionObject.GetInteraction().serializedInteractionData.isInteractable && useMark && interactionObject.ExclamationMark.activeSelf)
            {
                interactionObject.ExclamationMark.transform.position =
                    DataController.Instance.Cam.WorldToScreenPoint((Vector3) markOffset + interactionObject.transform.position);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            interactionObject.OnEnter();
        }

        private void OnTriggerExit(Collider other)
        {
            interactionObject.OnExit();
        }
    }
}

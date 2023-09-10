using UnityEngine;
using UnityEngine.Serialization;
using Utility.Core;
using Utility.Preference;
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
        [SerializeField] private Vector3 markOffset;
        [ConditionalHideInInspector("useMark")]
        [SerializeField] private bool isWorld;

        private void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
            if (useMark)
            {
                if (isWorld)
                {
                    interactionObject.ExclamationMark = Instantiate(markPrefab, PlayUIController.Instance.worldSpaceUI);
                }
                else
                {
                    interactionObject.ExclamationMark = Instantiate(markPrefab, PlayUIController.Instance.mapUi);
                }

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
                if (isWorld)
                {
                    interactionObject.ExclamationMark.transform.position = markOffset + interactionObject.transform.position;
                }
                else
                {
                    var screenPoint =
                        DataController.Instance.Cam.WorldToScreenPoint(interactionObject.transform.position + markOffset);
                    interactionObject.ExclamationMark.transform.position = screenPoint;
                }
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

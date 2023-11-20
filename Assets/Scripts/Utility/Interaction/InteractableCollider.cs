using UnityEngine;
using UnityEngine.Serialization;
using Utility.Core;
using Utility.UI;
using Utility.Utils.Property;

namespace Utility.Interaction
{
    public class InteractableCollider : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private InteractionObject interactionObject;
    
        [FormerlySerializedAs("useExclamationMark")] [Header("#Mark setting")]
        public bool useMark;
        [ConditionalHideInInspector("useMark")]
        [SerializeField] private GameObject markPrefab;
        [ConditionalHideInInspector("useMark")]
        [SerializeField] private Vector3 markOffset;
        [ConditionalHideInInspector("useMark")]
        [SerializeField] private bool isWorld;
#pragma warning restore 0649
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
            if (!useMark || !interactionObject.ExclamationMark.activeSelf ||
                !interactionObject.GetInteractionData().serializedInteractionData.isInteractable)
            {
                return;
            }
            
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

        private void OnTriggerEnter(Collider other)
        {
            interactionObject.OnColEnter();
        }

        private void OnTriggerExit(Collider other)
        {
            interactionObject.OnExit();
        }
    }
}

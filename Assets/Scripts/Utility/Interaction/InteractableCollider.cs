using UnityEngine;
using Utility.Property;
using Utility.System;

namespace Utility.Interaction
{
    public class InteractableCollider : MonoBehaviour
    {
        [SerializeField] private InteractionObject interactionObject;
    
        [Header("#Mark setting")]
        public bool useExclamationMark;
        [ConditionalHideInInspector("useExclamationMark")]
        [SerializeField] private Vector2 markOffset;

        private void Start()
        {
            if (useExclamationMark)
            {
                interactionObject.ExclamationMark = Instantiate(Resources.Load<GameObject>("Exclamation Mark"), DataController.instance.currentMap.ui);
                interactionObject.ExclamationMark.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (useExclamationMark)
            {
                Destroy(interactionObject.ExclamationMark);
            }
        }

        private void Update()
        {
            if (useExclamationMark && interactionObject.ExclamationMark.activeSelf)
            {
                interactionObject.ExclamationMark.transform.position =
                    (Vector3) markOffset + DataController.instance.cam.WorldToScreenPoint(interactionObject.transform.position);
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

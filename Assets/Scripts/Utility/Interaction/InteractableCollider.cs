using UnityEngine;
using UnityEngine.Serialization;
using Utility.Core;
using Utility.UI;
using Utility.Utils;
using Utility.Utils.Property;

namespace Utility.Interaction
{
    public class InteractableCollider : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private InteractionObject interactionObject;

        [FormerlySerializedAs("useExclamationMark")] [Header("#Mark setting")]
        public bool useMark;

        [ConditionalHideInInspector("useMark")] [SerializeField]
        private GameObject markPrefab;

        [ConditionalHideInInspector("useMark")] [SerializeField]
        private Vector3 markOffset;

        [ConditionalHideInInspector("useMark")] [SerializeField]
        private bool isWorld;
#pragma warning restore 0649
        private void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
            interactionObject.OnActiveAction = isActive =>
            {
                if (!useMark) return;
                ActiveExclamationMark(isActive);
            };
        }

        private void Update()
        {
            if (!useMark || !interactionObject.ExclamationMark ||
                !interactionObject.GetInteractionData().serializedInteractionData.isInteractable)
            {
                return;
            }

            if (isWorld)
            {
                interactionObject.ExclamationMark.transform.position =
                    markOffset + interactionObject.transform.position;
                var transformEulerAngles = interactionObject.ExclamationMark.transform.eulerAngles;
                transformEulerAngles.y = DataController.Instance.Cam.transform.rotation.y;
                interactionObject.ExclamationMark.transform.eulerAngles = transformEulerAngles;
            }
            else
            {
                var screenPoint =
                    DataController.Instance.Cam.WorldToScreenPoint(interactionObject.transform.position + markOffset);
                interactionObject.ExclamationMark.transform.position = screenPoint;
            }
        }

        private void ActiveExclamationMark(bool isActive)
        {
            if (isActive)
            {
                interactionObject.ExclamationMark = ObjectPoolHelper.Get(markPrefab);

                if (isWorld)
                {
                    interactionObject.ExclamationMark.transform.parent = PlayUIController.Instance.worldSpaceUI;
                }
                else
                {
                    interactionObject.ExclamationMark.transform.parent = PlayUIController.Instance.mapUi;
                }

                interactionObject.ExclamationMark.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                interactionObject.ExclamationMark.SetActive(true);
            }
            else
            {
                interactionObject.ExclamationMark.SetActive(false);
                interactionObject.ExclamationMark.transform.parent = null;
                ObjectPoolHelper.Release(markPrefab, interactionObject.ExclamationMark);
                interactionObject.ExclamationMark = null;
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

        private void OnDestroy()
        {
            if (interactionObject.ExclamationMark)
            {
                ActiveExclamationMark(false);
            }
        }
    }
}
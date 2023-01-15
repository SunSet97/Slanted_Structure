using UnityEngine;

namespace Utility.Interaction
{
    public class InteractableCollider : MonoBehaviour
    {
        [SerializeField] private InteractionObject interactionObject;
    
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

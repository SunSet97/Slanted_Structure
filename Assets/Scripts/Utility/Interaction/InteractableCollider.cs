using UnityEngine;

namespace Utility.Interaction
{
    public class InteractableCollider : MonoBehaviour
    {
        [SerializeField] private InteractionObject interactionObject;
    
        private void OnTriggerEnter(Collider other)
        {
        
        }

        private void OnTriggerExit(Collider other)
        {
        
        }
    }
}

using UnityEngine;

namespace Utility.Interaction
{
    public class InteractableCollider : MonoBehaviour
    {
        [SerializeField] private InteractionObj_stroke interactionObjStroke;
    
        private void OnTriggerEnter(Collider other)
        {
        
        }

        private void OnTriggerExit(Collider other)
        {
        
        }
    }
}

using UnityEngine;

namespace Utility.Interaction
{
    public class InteractionTrigger : InteractionObject
    {
        public void OnTriggerEnter(Collider other)
        {
            if (GetInteractionData().interactionMethod != InteractionMethod.Trigger)
            {
                return;
            }

            Debug.Log($"트리거 {gameObject}, 캐릭터: {other.transform.gameObject}");
            StartInteraction();
        }
    }
}
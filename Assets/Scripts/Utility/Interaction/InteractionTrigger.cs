using Data;
using UnityEngine;

namespace Utility.Interaction
{
    public class InteractionTrigger : InteractionObject
    {
        private bool isActivated;

        public void OnTriggerEnter(Collider other)
        {
            if (isActivated || interactionMethod != CustomEnum.InteractionMethod.Trigger)
            {
                return;
            }

            Debug.Log(gameObject.name + "트리거  " + other.transform.gameObject + isActivated);
            isActivated = true;
            StartInteraction();
        }
    }
}
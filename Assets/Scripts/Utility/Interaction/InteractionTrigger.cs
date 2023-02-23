using Data;
using UnityEngine;

namespace Utility.Interaction
{
    public class InteractionTrigger : InteractionObject
    {
        public void OnTriggerEnter(Collider other)
        {
            if (GetInteraction().interactionMethod != CustomEnum.InteractionMethod.Trigger)
            {
                return;
            }

            Debug.Log(gameObject.name + "트리거  " + other.transform.gameObject);
            StartInteraction();
        }
    }
}
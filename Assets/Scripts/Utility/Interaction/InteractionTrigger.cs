using Data;
using UnityEngine;

namespace Utility.Interaction
{
    public class InteractionTrigger : InteractionObject
    {
        private bool _isActivated;

        public void OnTriggerEnter(Collider other)
        {
            if (_isActivated || interactionMethod != CustomEnum.InteractionMethod.Trigger)
            {
                return;
            }

            Debug.Log(gameObject.name + "트리거  " + other.transform.gameObject + _isActivated);
            _isActivated = true;
            StartInteraction();
        }
    }
}
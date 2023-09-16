using System;
using UnityEngine;

namespace Episode.EP2.CatchPickpocket
{
    public class CatchPickpocketPlayer : MonoBehaviour
    {
        private Action<bool, Animator> onTrigger;

        public void Init(Action<bool, Animator> onTriggerAction)
        {
            onTrigger = onTriggerAction;
        }
            
        private void OnTriggerEnter(Collider other)
        {
            onTrigger?.Invoke(true, other.GetComponent<Animator>());
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Episode.EP2.ThrowCanGame
{
    public class Can : MonoBehaviour
    {
        public Action OnCollisionEnter;

        private float timer;

        private void Update()
        {
            if (timer <= 3f)
            {
                timer += Time.deltaTime;
                return;
            }

            OnCollisionEnter?.Invoke();
            enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!enabled || !other.CompareTag("item"))
            {
                return;
            }
            
            OnCollisionEnter?.Invoke();
            enabled = false;
        }
    }
}

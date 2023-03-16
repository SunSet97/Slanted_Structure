using System;
using UnityEngine;
using UnityEngine.Events;

namespace Episode.EP2.PlayGroundGame
{
    public class Can : MonoBehaviour
    {
        [NonSerialized]
        public UnityEvent OnCollisionEnter;

        private float timer;

        private void Awake()
        {
            OnCollisionEnter = new UnityEvent();
        }

        private void Update()
        {
            if (timer <= 3f)
            {
                timer += Time.deltaTime;
                return;
            }

            OnCollisionEnter?.Invoke();
            enabled = false;
            Destroy(gameObject);
            Debug.Log("하이");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!enabled)
            {
                return;
            }
        
            if (other.CompareTag("item") || other.CompareTag("floor"))
            {
                OnCollisionEnter?.Invoke();
                enabled = false;
            }
        }
    }
}

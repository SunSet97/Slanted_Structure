using System;
using UnityEngine;
using UnityEngine.Events;

public class Can : MonoBehaviour
{
    [NonSerialized]
    public UnityEvent onCollisionEnter;

    private void Awake()
    {
        onCollisionEnter = new UnityEvent();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enabled)
        {
            return;
        }
        
        if (other.CompareTag("item") || other.CompareTag("floor"))
        {
            onCollisionEnter?.Invoke();
            enabled = false;
        }
    }
}

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

    private void OnCollisionEnter(Collision other)
    {
        if (enabled && other.collider.tag == "floor || can" )
        {
            onCollisionEnter?.Invoke();
            enabled = false;
        }
    }
}

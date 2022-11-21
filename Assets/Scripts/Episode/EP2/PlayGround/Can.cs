using System;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Events;

public class Can : MonoBehaviour
{
    [NonSerialized]
    public UnityEvent onCollisionEnter;

    private float timer;

    private void Awake()
    {
        onCollisionEnter = new UnityEvent();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > 3f)
        {
            onCollisionEnter?.Invoke();
            enabled = false;
            Destroy(gameObject);
            Debug.Log("하이");
        }
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public Vector3 dir;
    public float force;

    // Start is called before the first frame update
    void Start()
    {
        Rigidbody myRigidbody = GetComponent<Rigidbody>();
        myRigidbody.AddForce(dir * force);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
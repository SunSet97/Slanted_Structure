using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct ThrowCan
{
    public float power;
    [Range(0, 1)]
    public float yplus;
}
public class testthree : MonoBehaviour
{
    enum a { per, t, no}
    a aa;
    public Transform destination;
    public ThrowCan[] throwCans; 
    
    // Start is called before the first frame update
    void Awesome(a type)
    {
        Vector3 direction = (destination.position - transform.position).normalized;
        //Debug.Log(direction);
        direction.y = throwCans[(int)type].yplus;
        //Debug.Log(direction.normalized);
        GetComponent<Rigidbody>().AddForce(direction.normalized * throwCans[(int)type].power, ForceMode.Impulse);
        GetComponent<Rigidbody>().AddTorque(direction.normalized * throwCans[(int)type].power, ForceMode.Impulse);
        //GetComponent<Rigidbody>().AddForce(Vecor3(1,1,0) * 10, ForceMode.Impulse);

    }

    private void Start()
    {
        Awesome(a.per);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

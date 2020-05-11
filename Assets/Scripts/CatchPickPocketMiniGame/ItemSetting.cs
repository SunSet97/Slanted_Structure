using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSetting : MonoBehaviour
{
    float rotateSpeed = 60.0f;

    
    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);      
    }



}

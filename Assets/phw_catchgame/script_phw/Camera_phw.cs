using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_phw : MonoBehaviour
{
    public GameObject target;
    public GameObject RauForOffsetSetting;
    Vector3 offset;


    void Start()
    {
        offset = transform.position - RauForOffsetSetting.transform.position;
    }

    void Update()
    {
        transform.position = target.transform.position + offset;
    }
}

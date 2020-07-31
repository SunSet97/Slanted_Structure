using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapData : MonoBehaviour
{
    public GameObject map;
    public string mapCode;
    public string playMethod;
    public Vector3 camPos;
    public string character;

    void Start()
    {
        map = GetComponentsInChildren<Transform>()[1].gameObject;
        
        mapCode = this.gameObject.name;
    }
}

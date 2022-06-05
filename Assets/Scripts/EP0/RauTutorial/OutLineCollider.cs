using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutLineCollider : MonoBehaviour
{
    [Header("#Outline setting")]
    public Color outlineColor = Color.red;

    public Vector3 offset;
    public float radius;
    public Outline.Mode mode = Outline.Mode.OutlineAll;
    public Outline outline;

    void Start()
    {
        if(outline == null)
            outline = gameObject.AddComponent<Outline>();
        outline.OutlineMode = mode;
        outline.OutlineColor = outlineColor;
        outline.OutlineWidth = 8f;
        outline.enabled = false;
        SphereCollider sphereCol;
        if (!gameObject.TryGetComponent(out sphereCol)) 
            sphereCol = gameObject.AddComponent<SphereCollider>();  // 콜라이더 없으면 자동 추가
        sphereCol.isTrigger = true;
        sphereCol.center = offset / transform.lossyScale.y;
        sphereCol.radius = radius / transform.lossyScale.y;
    }

    void OnTriggerEnter(Collider other)
    {
        outline.enabled = true;
    }
    void OnTriggerExit(Collider other)
    {
        outline.enabled = false;
    }
}

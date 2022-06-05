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
        offset /= transform.lossyScale.y;
        radius /= transform.lossyScale.y;
        sphereCol.center = offset;
        sphereCol.radius = radius;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = outlineColor - Color.black * 0.7f;
        Gizmos.DrawWireSphere(transform.position + offset, radius);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        outline.enabled = true;
        Debug.Log("키기");
    }
    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        outline.enabled = false;
        Debug.Log("끄기");
    }
}

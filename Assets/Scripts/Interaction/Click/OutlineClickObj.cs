using System;
using CommonScript;
using UnityEngine;

public class OutlineClickObj : MonoBehaviour, IClickable
{
    public bool IsClickEnable
    {
        get => outline.enabled;
        set
        {
            outline.enabled = value;
            ActiveObjectClicker(value);
        }
    }

    public bool IsClicked { get; set; }
    
    [Header("#Outline setting")]
    public Color outlineColor = Color.red;

    public Vector3 offset;
    public float radius;
    public Outline.Mode mode = Outline.Mode.OutlineAll;
    public Outline outline;

    public void ActiveObjectClicker(bool isActive)
    {
        if (!ObjectClicker.instance)
        {
            return;
        }
        ObjectClicker.instance.UpdateClick(this, isActive);
    }

    public bool GetIsClicked()
    {
        if (IsClicked)
        {
            IsClicked = false;
            return true;
        }

        return false;
    }
    public void Click()
    {
        if (!IsClickEnable) return;
        IsClicked = true;
    }

    void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
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
        radius /= transform.lossyScale.y;
        sphereCol.center = transform.InverseTransformPoint(transform.position + offset);
        sphereCol.radius = radius;
    }
    

    void OnDrawGizmos()
    {
        Gizmos.color = outlineColor - Color.black * 0.7f;
        Gizmos.DrawWireSphere(transform.position + offset, radius);
    }

    void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Player") || !enabled) return;
        IsClickEnable = true;
        Debug.Log("키기 - " + other.gameObject);
    }
    void OnTriggerExit(Collider other)
    {
        if(!other.CompareTag("Player")) return;
        IsClickEnable = false;
        Debug.Log("끄기");
    }

    private void OnDisable()
    {
        if (!Application.isPlaying) return;
        IsClickEnable = false;
    }
    private void OnDestroy()
    {
        if (!Application.isPlaying) return;
        IsClickEnable = false;
    }
}

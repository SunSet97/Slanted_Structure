using UnityEngine;

/// <summary>
/// 단순 아웃라인 시각화용 스크립트 
/// </summary>
public class OutlineCollider : MonoBehaviour
{
    
    [Header("#Outline setting")]
    public Color outlineColor = Color.red;

    public Vector3 offset;
    public float radius;
    public Outline.Mode mode = Outline.Mode.OutlineAll;
    public Outline outline;

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
        if(outline == null)
            outline = gameObject.AddComponent<Outline>();
        outline.OutlineMode = mode;
        outline.OutlineColor = outlineColor;
        outline.OutlineWidth = 8f;
        outline.enabled = false;
        if (!gameObject.TryGetComponent(out SphereCollider sphereCol)) 
            sphereCol = gameObject.AddComponent<SphereCollider>();  // 콜라이더 없으면 자동 추가
        sphereCol.isTrigger = true;
        radius /= transform.lossyScale.y;
        sphereCol.center = transform.InverseTransformPoint(transform.position + offset);
        sphereCol.radius = radius;
    }
    

    // void OnDrawGizmos()
    // {
    //     Gizmos.color = outlineColor - Color.black * 0.7f;
    //     Gizmos.DrawWireSphere(transform.position + offset, radius * transform.lossyScale.y);
    // }

    void OnTriggerEnter(Collider other)
    {
        outline.enabled = true;
        Debug.Log("키기");
    }
    void OnTriggerExit(Collider other)
    {
        outline.enabled = false;
        Debug.Log("끄기");
    }
}

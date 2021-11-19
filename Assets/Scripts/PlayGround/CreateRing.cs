using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateRing : MonoBehaviour
{
    public int segments;
    public float noteSpeed;

    LineRenderer line;

    public void Setup(Vector3 nodeScale)
    {
        transform.localScale = nodeScale;
        line = gameObject.GetComponent<LineRenderer>();
        line.SetVertexCount(segments +11);
        line.useWorldSpace = false;
        CreatePoints();
    }

    void Update()
    {
        
        transform.localScale = new Vector3(transform.localScale.x - 0.1f * noteSpeed * Time.deltaTime,
                                           transform.localScale.y - 0.1f * noteSpeed * Time.deltaTime, 1);
    }

    void CreatePoints()
    {
        float x;
        float y;
        float z = 0f;

        float angle = 0f;

        for (int i = 0; i <= (segments + 10); i++)
        {
            x = Mathf.Cos(Mathf.Deg2Rad * angle) *0.5f;
            y = Mathf.Sin(Mathf.Deg2Rad * angle) * 0.5f;

            line.SetPosition(i, new Vector3(x, y, z));
            //line.transform.localPosition = new Vector3(190, 0, 0);
            //line.transform.localScale = new Vector3(100, 100, 0);
            angle += (360f / segments);
        }
    }
     
}

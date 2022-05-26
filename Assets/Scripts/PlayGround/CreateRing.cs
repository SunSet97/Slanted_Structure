using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateRing : MonoBehaviour
{
    public int segments;
    public float noteSpeed;
    [System.NonSerialized]
    public float radius;
    private float nodeRadius;
    [System.NonSerialized]
    public float width;


    private LineRenderer line;
    public GameObject Ring;
    
    public void Setup()
    {
        Vector3 vec = transform.localPosition;
        vec.z = -.001f;
        transform.localPosition = vec;
        gameObject.SetActive(true);

        nodeRadius = Ring.GetComponent<RectTransform>().localScale.x / 2 * Ring.GetComponent<UnityEngine.UI.Image>().sprite.rect.width / Ring.GetComponent<UnityEngine.UI.Image>().sprite.pixelsPerUnit;
        radius = nodeRadius;

        //transform.localScale = nodeScale;
        line = GetComponent<LineRenderer>();
        line.positionCount = segments + 50;
        line.useWorldSpace = false;

        //CreatePoints();
        width = 0.10f;
        line.endWidth = width;
        line.startWidth = width;
    }

    void Update()
    {
        //radius -= 0.1f * noteSpeed * Time.deltaTime;
        
            radius -= nodeRadius / noteSpeed * Time.deltaTime;
            CreatePoints();

        //if (radius < Ring.transform.localScale.x / 2 * 0.3) {
        //    //Disappear Ring
        //    transform.parent.gameObject.SetActive(false);
        //}
        //transform.localScale = new Vector3(transform.localScale.x - 0.1f * noteSpeed * Time.deltaTime,
                                           //transform.localScale.y - 0.1f * noteSpeed * Time.deltaTime, 1);
    }

    public void CreatePoints()
    {
        //float x;
        //float y;
        //float z = - width / 2f;

        //float angle = 0f;

        //for (int i = 0; i <= (segments + 10); i++)
        //{
        //    x = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
        //    y = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;

        //    line.SetPosition(i, new Vector3(x, y, z));
        //    //line.transform.localPosition = new Vector3(190, 0, 0);
        //    //line.transform.localScale = new Vector3(100, 100, 0);
        //    angle += (360f / segments);
        //}

        for (int i = 0; i < segments + 50; i++)
        {
            line.SetPosition(i, radius * new Vector3(Mathf.Cos(Mathf.Deg2Rad * 360 / segments * i), Mathf.Sin(Mathf.Deg2Rad * 360 / segments * i)));
        }

    }
    //public void DisappearCircle() {
    //    transform.parent.gameObject.SetActive(false);
    //}

}

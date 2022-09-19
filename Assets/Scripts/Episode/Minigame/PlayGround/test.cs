using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public int vertexCount;
    [System.NonSerialized]
    public float recentRadius;
    public float countSecond;
    private float entireRadius;
    public float width;
    public GameObject bigCircle;

    private LineRenderer line;

    //원 생성
    // 1. 꼭짓점 갯수의 배열 초기화
    //linerenderer 불러오기
    //갯수 설정
    public void InitializeCircle()
    {
        gameObject.SetActive(true);
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = false;

        line.positionCount = vertexCount+50;
        line.startWidth = width;
        line.endWidth = width;
        entireRadius = bigCircle.GetComponent<RectTransform>().localScale.x / 2 * bigCircle.GetComponent<UnityEngine.UI.Image>().sprite.rect.width / bigCircle.GetComponent<UnityEngine.UI.Image>().sprite.pixelsPerUnit;
        recentRadius = entireRadius;
    }

    // 2. 꼭짓점 배열의 값을 바꾸기
    public void UpdateVertex()
    {
        //Debug.Log(recentRadius);
        for (int i = 0; i < vertexCount+50; i++)
        {
            //Debug.Log(new Vector3(Mathf.Cos(Mathf.Deg2Rad * 360 / vertexCount*i), Mathf.Sin(Mathf.Deg2Rad * 360 / vertexCount * i)));
            line.SetPosition(i, recentRadius * new Vector3(Mathf.Cos(Mathf.Deg2Rad * 360 / vertexCount * i), Mathf.Sin(Mathf.Deg2Rad * 360 / vertexCount * i)));
        }
    }


    // 반지름 줄어듬
    public void Update()
    {
        recentRadius -= entireRadius / countSecond * Time.deltaTime;
        UpdateVertex();
        //Debug.Log(bigCircle.transform.localScale.x / 2 * 0.3 + " " + recentRadius);
        if (recentRadius < bigCircle.transform.localScale.x / 2 * 0.3)
            DisapearCircle();
    }
    // (일정 크기 이하) 사라짐
    public void DisapearCircle()
    {
        transform.parent.gameObject.SetActive(false);
        //gameObject.SetActive(false);
    }
}

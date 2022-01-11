using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testtwo : MonoBehaviour
{
    public GameObject circle;
    // Start is called before the first frame update
    void Start()
    {
        //게임 실행 
        CanvasControl.instance_CanvasControl.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
        CanvasControl.instance_CanvasControl.GetComponent<Canvas>().worldCamera = Camera.main;
        CanvasControl.instance_CanvasControl.GetComponent<Canvas>().planeDistance = 0.8f;
        // 원 이미지 보이게
        circle.SetActive(true);
        // 움직이는 원 생성
        circle.transform.GetComponentInChildren<test>().InitializeCircle();
        circle.transform.GetComponentInChildren<test>().UpdateVertex();
        
    }

    //버튼 누르기
    public void ClickButton()
    {
         test circleParent = circle.transform.GetComponentInChildren<test>();
        //범위 나누기
        if (circleParent.recentRadius * 2 < circle.transform.GetChild(2).localScale.x)
        {
            Debug.Log("Best Range");
        }
        else if (circleParent.recentRadius * 2 < circle.transform.GetChild(0).localScale.x)
        {
            Debug.Log("Good Range");
        }
        else if (circleParent.recentRadius * 2 < circle.transform.GetChild(1).localScale.x)
        {
            Debug.Log("Bad Range"); 
        }
        else
        {
            Debug.Log(" ");
        }
        //실행 멈추기
        //카메라 돌려놓기
        circle.SetActive(false);
        CanvasControl.instance_CanvasControl.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

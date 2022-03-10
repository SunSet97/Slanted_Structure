using System.Collections;
using System.Collections.Generic;
using Play;
using UnityEngine;
using UnityEngine.UI;

public class PlayGroundManager : MonoBehaviour, IPlayable
{
    public bool IsPlay { get; set; } = false;

    public RectTransform[] range;
    public test ring;

    public float radius;
    public int segments;
    public float speed;
    public float width;

    private void Start()
    {
        Startplaying();
    }

    public void Play()
    {
        IsPlay = true;
    }

    public void EndPlay()
    {
        IsPlay = false;
    }
    
    //원 생성
    public void Startplaying()
    {
        CanvasControl.instance_CanvasControl.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
        CanvasControl.instance_CanvasControl.GetComponent<Canvas>().worldCamera = DataController.instance_DataController.cam;
        CanvasControl.instance_CanvasControl.GetComponent<Canvas>().planeDistance = 1;
        //ring.Setup(radius *range[2].localScale.x / 2, CanvasControl.instance_CanvasControl.GetComponent<Canvas>().planeDistance * 0.03f * width, speed, segments);
       
        DataController.instance_DataController.currentMap.ui.SetActive(true);
    }

    //원 판정함수
    public void ClearCheck()
    {
        //(2.5 < ) 경우 = 완전히 실패
        //(2 < ) 경우 = Bad
        //(1.5 < ) 경우 = Good
        //(1 < ) 경우 = Best
        if (range[2].localScale.x < ring.recentRadius * 2 * ring.transform.localScale.x)
        {
            Debug.Log("앗! 안들어감");
            EndPlaying();
        }
        else if (range[1].localScale.x < ring.recentRadius * 2 * ring.transform.localScale.x)
        {
            Debug.Log("아깝다! 한번 튕기고 안들어감");
            EndPlaying();
        }
        else if (range[0].localScale.x < ring.recentRadius * 2 * ring.transform.localScale.x)
        {
            Debug.Log("아싸! 한번 튕기고 들어감");
            EndPlaying();
        }
        else
        {
            Debug.Log("유후! 바로 들어감");
            EndPlaying();
        }
        //if(ring.transform.localScale.x = range[0].)
    }

    //원 사라짐
    public void Update()
    {
        //원 사라지는 것 체크 ( < 1인 경우)
        //실패 판정 한 경우 게임 멈추는 함수 실행
        if (range[3].localScale.x > ring.recentRadius * 2 * ring.recentRadius * 2 * ring.transform.localScale.x)
        {
            Debug.Log("실패");

            //실패시 게임 멈추기
            //EndPlaying();
        }
    }

    //게임 끝날 경우 함수
    public void EndPlaying()
    {
        CanvasControl.instance_CanvasControl.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        IsPlay = false;
        DataController.instance_DataController.currentMap.ui.SetActive(false);

    }
}

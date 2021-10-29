using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayGroundManager : MonoBehaviour, Playable
{
    public bool isPlay { get; set; } = false;

    public RectTransform[] range;
    public CreateRing ring;



    public GameObject trash;
    Rigidbody trash_rigid;
    public Button innerCircle;
    public Image outterCircle;
    public float speed;
    public Text text; // 판정 텍스트
    Vector3 originotterCircleScale;
    bool isTouchBtn = false;
    bool isTriggerCircleBtns = false;

    //void Start()
    //{
        //originotterCircleScale = outterCircle.transform.localScale;

        ////DataController.instance_DataController.isMapChanged = true;

        //// 버튼 안보이게!
        //outterCircle.gameObject.SetActive(false);
        //innerCircle.gameObject.SetActive(false);

        //// 뭔 터치 인터렉션 전에 하는 대화
        //DataController.instance_DataController.LoadData("PlayGround", "playground.json");
        ////수정
        ////CanvasControl.instance_CanvasControl.StartConversation();

        //// 쓰레기 리지드바디
        //trash_rigid = trash.GetComponent<Rigidbody>();
    //}


    // Update is called once per frame
    //void Update()
    //{

    //    //// 대화가 끝난 후 StartCoroutine(WaitTouch())가 한번만 실행되도록
    //    //if (CanvasControl.instance_CanvasControl.dialogueCnt == CanvasControl.instance_CanvasControl.dialogueLen && !isTriggerCircleBtns && CanvasControl.instance_CanvasControl.endConversation)
    //    //{
    //    //    print("대화 끝!");
    //    //    CanvasControl.instance_CanvasControl.endConversation = false;
    //    //    isTriggerCircleBtns = true;
    //    //    outterCircle.gameObject.SetActive(true);
    //    //    innerCircle.gameObject.SetActive(true);
    //    //    StartCoroutine(WaitTouch());
    //    //}

    //    //// 대화가 끝나고 버튼 커졌다가 작아졌다가!
    //    //if (isTriggerCircleBtns && !isTouchBtn)
    //    //{
    //    //    outterCircle.transform.localScale -= new Vector3(speed, speed, 0) * Time.deltaTime;
    //    //    if (outterCircle.transform.localScale.x < 145)
    //    //    {
    //    //        outterCircle.transform.localScale = originotterCircleScale;
    //    //    }
    //    //}



    //}

    // 겜판정
    void GameJudgment()
    {

        /* 2r은 바깥원
        유후: 145 <= 2r <= 160 바로 들어감
        아싸: 160 < 2r <= 200 한번 튕기고 들어감
        아깝다: 200 < 2r <= 240 한번 튕기고 안들어감
        앗: 240 < 2r <= 300 걍 안들어감
        */

        float touchOutterCircleScaleX = outterCircle.transform.localScale.x; // 터치했을 때 바깥원 좌, 우 크기

        if (145 <= touchOutterCircleScaleX && touchOutterCircleScaleX <= 160f)
        {
            print("유후! 캔 바로 들어감");
            ThrowTrash(1);
            isTouchBtn = true;
        }
        else if (160 < touchOutterCircleScaleX && touchOutterCircleScaleX <= 200)
        {
            print("아싸! 캔 한번 튕기고 들어감");
            ThrowTrash(1);
            isTouchBtn = true;
        }
        else if (200 < touchOutterCircleScaleX && touchOutterCircleScaleX <= 240)
        {
            print("아깝다! 한번 튕기고 안들어감");
            ThrowTrash(1);
            isTouchBtn = true;
        }
        else if (240 < touchOutterCircleScaleX && touchOutterCircleScaleX <= 300)
        {
            print("앗! 안들어감");
            ThrowTrash(1);
            isTouchBtn = true;
        }

    }

    void ThrowTrash(int dgree)
    {
        trash_rigid.AddForce(new Vector3(1, 1, 1));
        print("에?");
    }

    /* 만약 이펙트 있으면 이거 사용하고 게임판정에 넣기
    void BtnEffect(ColorBlock color) {
        ColorBlock outterBtnColor = outterCircle.GetComponent<ColorBlock>();
        outterCircle.colors = color;
    }
    */


    IEnumerator WaitTouch()
    {
        yield return new WaitUntil(() => isTouchBtn); // 터치했으면 끄읕
        yield return new WaitForSeconds(1); // 1초후 
        // 버튼 안보이게하기
        outterCircle.gameObject.SetActive(false);
        innerCircle.gameObject.SetActive(false);
    }

    // 버튼 터치
    public void TouchInnerrCircle()
    {
        GameJudgment();
    }

    private void Start()
    {
        Startplaying();
    }

    //원 생성
    public void Startplaying()
    {
        ring.Setup(range[2].localScale * 2);
        DataController.instance_DataController.currentMap.ui.SetActive(true);

    }

    //원 판정함수
    public void ClearCheck()
    {
        //(2.5 < ) 경우 = 완전히 실패
        //(2 < ) 경우 = Bad
        //(1.5 < ) 경우 = Good
        //(1 < ) 경우 = Best
        if (range[2].localScale.x < ring.transform.localScale.x)
        {
            Debug.Log("앗! 안들어감");
            EndPlaying();
        }
        else if (range[1].localScale.x < ring.transform.localScale.x)
        {
            Debug.Log("아깝다! 한번 튕기고 안들어감");
            EndPlaying();
        }
        else if (range[0].localScale.x < ring.transform.localScale.x)
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
        if (range[3].localScale.x > ring.transform.localScale.x)
        {
            Debug.Log("실패");

            //실패시 게임 멈추기
            EndPlaying();
        }
    }

    //게임 끝날 경우 함수
    public void EndPlaying()
    {
        isPlay = false;
        DataController.instance_DataController.currentMap.ui.SetActive(false);
    }
}

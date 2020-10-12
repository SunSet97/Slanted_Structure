using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayGroundManager : MonoBehaviour
{
    public Button innerCircle;
    public Image outterCircle;
    public float speed;
    public Text text; // 판정 텍스트
    Vector3 originotterCircleScale;
    bool isTouchBtn = false;

    void Start()
    {
        originotterCircleScale = outterCircle.transform.localScale;
        StartCoroutine(WaitTouch());
    }


    // Update is called once per frame
    void Update()
    {
        if (!isTouchBtn)
        {
            outterCircle.transform.localScale -= new Vector3(speed, speed, 0) * Time.deltaTime;
            if (outterCircle.transform.localScale.x < 145)
            {
                outterCircle.transform.localScale = originotterCircleScale;
            }
        }


    }

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
            isTouchBtn = true;
        }
        else if (160 < touchOutterCircleScaleX && touchOutterCircleScaleX <= 200)
        {
            print("아싸! 캔 한번 튕기고 들어감");
            isTouchBtn = true;
        }
        else if (200 < touchOutterCircleScaleX && touchOutterCircleScaleX <= 240)
        {
            print("아깝다! 한번 튕기고 안들어감");
            isTouchBtn = true;
        }
        else if (240 < touchOutterCircleScaleX && touchOutterCircleScaleX <= 300)
        {
            print("앗! 안들어감");
            isTouchBtn = true;
        }

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
}

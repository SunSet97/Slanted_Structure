using System.Collections;
using System.Collections.Generic;
using Play;
using UnityEngine;
using UnityEngine.UI;

public class TossCoin : MonoBehaviour, IGamePlayable
{
    public GameObject coin; // 동전 프리펩
    public Transform arrow; // 화살표
    private float time; // 타이머
    private int step = 0; // 던지는 단계

    [Header("-Toss Direction Ctrl")]
    public float rotateSpd = 1.5f; // 화살표 상하 움직임 속도
    private Vector3 tossDir; // 던지는 방향

    [Header("-Toss Force Ctrl")]
    public float gaugeSpd = 3f; // 힘 조절 속도
    public float maxPow = 55f; // 최대 힘
    public float minPow = 45f; // 최소 힘
    private float tossPow; // 던지는 힘

    [Header("-About Game")]
    public Text tryNumText; // 시도 횟수 UI 텍스트
    public int tryNum = 0; // 시도 횟수
    public Text passText; // 성공 UI 텍스트
    public bool isPass = false; // 성공 여부

    void Update()
    {
        if (IsPlay)
        {
            if (!isPass) // 미성공시 실행
            {
                tryNumText.text = "시도 횟수 : " + tryNum.ToString(); // 시도 횟수 갱신

                if (step == 0) // 화살표 각도 회전
                {
                    time += Time.deltaTime * rotateSpd;
                    transform.eulerAngles = Vector3.forward * (Mathf.Sin(time) * 45 + 45);
                }
                else if (step == 1) // 화살표 길이 변화
                {
                    time += Time.deltaTime * gaugeSpd;
                    arrow.localScale = new Vector3(0.1f, Mathf.Sin(time) * 0.045f + 0.055f, 0.1f);
                }
                else
                {
                    arrow.localScale = Vector3.one * 0.1f; // 화살표 초기화
                    time = 270 * Mathf.Deg2Rad; // 타이머 초기화
                    step = 0; // 단계 초기화
                }

                if (tryNum >= 5)
                    passText.text = " 실 패 !!! ";
            }
            else
            {
                passText.text = " 성 공 !!! ";
            }

            //if (Input.GetButtonDown("Fire1")) TossTheCoin(); // 테스트용(마우스 좌클릭시 실행)
        }
    }

    // 동전을 던져라(버튼 누를 시 실행)
    public void TossTheCoin()
    {
        if (tryNum < 5 && !isPass)
        {
            switch (step)
            {
                case 0:
                    tossDir = (arrow.up).normalized; // 던질 방향 설정
                    time = 270 * Mathf.Deg2Rad; // 타이머 초기화
                    step++; // 다음 단계
                    break;
                case 1:
                    tossPow = (maxPow + minPow) / 2 + (maxPow - minPow) / 0.09f * (arrow.localScale.y - 0.045f); // 던지는 힘 설정
                    arrow.localScale = Vector3.one * 0.1f; // 화살표 초기화
                    time = 270 * Mathf.Deg2Rad; // 타이머 초기화
                    step = 0; // 단계 초기화
                    tryNum++; // 시도 횟수 증가
                    GameObject tossedCoin = Instantiate(coin, transform.position, Quaternion.identity); // 던질 동전 만들기
                    tossedCoin.GetComponent<Coin>().dir = tossDir;
                    tossedCoin.GetComponent<Coin>().force = tossPow;
                    break;
                default:
                    arrow.localScale = Vector3.one * 0.1f; // 화살표 초기화
                    time = 270 * Mathf.Deg2Rad; // 타이머 초기화
                    step = 0; // 단계 초기화
                    break;
            }
        }
    }

    public bool IsPlay { get; set; }
    public void Play()
    {
        IsPlay = true;
    }

    public void EndPlay()
    {
        IsPlay = false;
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MiniGameManager : MonoBehaviour
{
    [Header("라우")]
    public CharacterManager rau;
    public float rauSpeed;
    public float acceleration;
    public bool conflict = false; // 일단 파티클때문에 남겨놓음. 다른 방법 있으면 삭제하기..
    public ParticleSystem particle;

    [Header("PickPocket")]
    public GameObject pickPocket;
    public float pickPocketSpeed;

    [Header("Kickboard")]
    public GameObject kickboard;
    public GameObject kickboardSpeat; // 킥보드 타는 스핏.
    Vector3 kickboardDir;
    Vector3 kickboardMovVec;
    bool kickboardLocated = false;
    int kickboardNum;
    float kickboardSpeed = 3.5f;

    [Header("Dove")]
    public Transform doveParent;
    //Transform[] doves; @비둘기 하나하나 사용할 때 쓰기.
    Vector3 flyDir = new Vector3(1, 1, 1);
    float flySpeed = 0.5f;
    bool flyTrigger = false;
    bool conflictDove = false;

    [Header("Timer")]
    public float timer;
    public Text textTimer;
    //public Text textItem;

    [Header("판정")]
    public float successDistance; // 라우~소매치기 거리가 어느정도 되야 성공하는지.

    bool start = false;
    bool begin = false;
    Coroutine initialCor;
    float tempSpeed;
    public GameObject peopleParent;
    public bool isStopping = false;
    float initialRauSpeed;

    void Start()
    {
        //doves = doveParent.GetComponentInChildren<Transform>(); @비둘기 하나하나 사용할 때 쓰기
        //textTimer.gameObject.SetActive(false);
        particle.Play();
        initialRauSpeed = rauSpeed;
    }

    void Update()
    {
        //if (DataController.instance_DataController.mapCode == "010001" && !start)
        if (!start)
        {
            StartCoroutine(Wait(3.0f)); // 3초 후 시작.
        }
        //else if (DataController.instance_DataController.mapCode == "010001" && start && begin)
        else if (start && begin)
        {
            // 미니게임메이저 이동
            transform.position = rau.transform.position + new Vector3(0,0.5f,0);

            // 파티클 이동
            particle.transform.position = rau.transform.position + new Vector3(0, 1.5f, 10);
            // 라우 이동
            rau.ctrl.Move(new Vector3(0, 0, 1) * rauSpeed * Time.deltaTime);
            rauSpeed += acceleration * Time.deltaTime; // 라우 가속도 운동
            // 소매치기 이동
            pickPocket.transform.Translate(new Vector3(0, 0, 1) * pickPocketSpeed * Time.deltaTime);

            if (kickboardLocated)
            {
                kickboard.transform.Translate(kickboardDir);
                kickboardSpeat.transform.position = kickboard.transform.position;
            }


            /*
            if (rau.isCollisionObstacle && !isStopping)
            {
                if (rau.hitObj.name == "Pigeon_collection" || rau.hitObj.transform.parent.gameObject.name == "Pigeon_collection")
                {
                    StartCoroutine(Fly(8.0f, 8.0f));
                }  //충돌된 obstacle이 비둘기인지 확인하는 if문
                else
                {
                    isStopping = true;
                    StartCoroutine(Stop(10.0f, rau.hitObj)); // Obstacle이랑 부딪히면, 3초간 멈춤
                }

            }
            */


            /*if (rau.isContactPigeon) {
                rau.isContactPigeon = false;
                StartCoroutine(Fly(8.0f, 8.0f));
            }*/



            if (flyTrigger)
            {
                doveParent.Translate(flyDir * flySpeed * Time.deltaTime);
                print("dove_parent 이동하자");
            }

            // 판정
            timer -= Time.deltaTime; // 타이머
            //textTimer.text = "남은 시간: " + Mathf.Round(timer);
            CheckSuccess(); // 성공인지 아닌지 판정.

        }


    }

    private void CheckSuccess() // 게임 결과 판정 메소드
    {
        if (Mathf.Round(timer) == 0)
        {
            if (Vector3.Distance(rau.transform.position, pickPocket.transform.position) <= successDistance)
            {
                Debug.Log("성공.");
            }
            else
            {
                Debug.Log("실패.");
            }
        }
        else
        {
            if (Vector3.Distance(rau.transform.position, pickPocket.transform.position) <= successDistance)
            {
                Debug.Log("성공");
            }
        }
    }

    private void InitialSetting() // 초기 장애물 배치 세팅
    {
        start = true;
        pickPocket.SetActive(true);
        //textTimer.gameObject.SetActive(true); // 타이머 보이게 하기
        SetDataController(); // 데이터 컨트롤러 정보 세팅
        SetPickPocket(); // 소매치기 위치 세팅
        SetKickBoard(); // 킥보드 위치 세팅
        SetPeople(); // 사람(장애물) 위치 세팅
        //StartCoroutine(Fly(5.0f, 5.0f)); // 비둘기 5초동안 움직임. 5초동안 대기.
        StartCoroutine(KickboardCycle(4.0f, 8.0f)); // 킥보드 2초동안 움직임. 8초에 한번씩 킥보드 위치시킴.
        //rau.ctrl.Move(new Vector3(0, 0, 1) * rauSpeed * Time.deltaTime);
        begin = true;
    }

    private void SetDataController()
    {   // 카메라 설정
        DataController.instance_DataController.camDis_x = 0;
        DataController.instance_DataController.camDis_y = 1.3f;
        DataController.instance_DataController.camDis_z = -2.5f;
    }

    private void SetPickPocket()
    {
        // 소매치기 위치 초기 세팅 - 라우 위치와 z방향으로 distance만큼 떨어진 위치에 세팅.
        float distance = 5.0f; // 초기 소매치기 ~ 라우 거리
        pickPocket.transform.position = rau.transform.position + new Vector3(0, 0.5f, distance);//new Vector3(rauPos.x , rauPos.y, rauPos.z + distance);// 소매치기 위치 초기화. (== 소매치기 시작 위치.)

    }

    private void SetKickBoard()
    {
        float x = 10.0f;
        float z = 10.0f;
        // 전동 킥보드 위치 세팅
        kickboardNum = Random.Range(-1, 2); // -1: 왼쪽에서 등장  0: 정면에서 등장  1: 오른쪽에서 등장
        kickboard.transform.position = rau.transform.position + new Vector3(kickboardNum * x, 0, z);

        if (kickboardNum == 0) // 정면에서 오른쪽에서
        {
            kickboardDir = new Vector3(0, 0, -kickboardSpeed * Time.deltaTime);

        }
        else if (kickboardNum == 1) // 오른쪽에서 등장
        {
            kickboardDir = new Vector3(-kickboardNum * kickboardSpeed * Time.deltaTime, 0, 0);
        }
        else
        {
            kickboardDir = new Vector3(-kickboardNum * kickboardSpeed * Time.deltaTime, 0, 0);
        }

        kickboardLocated = true;
    }

    private void setDoves()
    {
        /*float distance = 0.5f; // 비둘기끼리 따닥따닥 안붙어있게 위치시키기위함.
        for (int i = 1; i < doves.Length; i++) {
            doves[i].position = rau.transform.position + new Vector3(0, 0, 15.0f); // 라우와 15정도 떨어진 위치에 위치시키기.
        } @ 비둘기 따로따로 쓸때, 따로따로 배치하고 싶을 때 사용하기*/
        doveParent.position = rau.transform.position + new Vector3(0, 0, 15.0f); // 라우와 15 떨어진 위치에 위치시키기.
    }

    private void SetPeople()
    {
        //gameObject.child
        print("SetPeople()");

    }

    IEnumerator Stop(float time, GameObject other) // 충돌 시 멈춤.
    {
        if (isStopping)//isStopping이 참일 때만 활성화.
        {
            DataController.instance_DataController.joyStick.gameObject.SetActive(false); // 조이스틱 없애기
            float tempAcceleration = acceleration;
            //float tempSpeed = rauSpeed;
            rauSpeed = 0;
            acceleration = 0;
            yield return new WaitForSeconds(time);
            DataController.instance_DataController.joyStick.gameObject.SetActive(true); // 조이스틱 생기게
            rauSpeed = initialRauSpeed; // 장애물과 충돌하면, 라우 스피트는 다시 초기화!
            acceleration = tempAcceleration; // 장애물과 충돌하면, 라우 가속도 다시 초기화!
            //rau.isCollisionObstacle = false;
            isStopping = false;//장애물 사라지고 나서 거짓으로 초기화
        }
;
    }

    IEnumerator Fly(float movingTime, float waitingTime) // 비둘기 날아가는거
    {
        doveParent.gameObject.SetActive(true);
        flyTrigger = true;
        yield return new WaitForSeconds(5.0f); // 5초동안 날아감.
        flyTrigger = false;
        doveParent.gameObject.SetActive(false);
        yield return new WaitForSeconds(5.0f); // 5초동안 대기했다가 다음 위치로 배치됨.
        setDoves();
        flyTrigger = true;
    }

    IEnumerator KickboardCycle(float movingTime, float waitingTime)
    {
        while (true)
        {
            if (DataController.instance_DataController.mapCode != "010001") break;
            kickboard.SetActive(true);
            kickboardSpeat.SetActive(true);
            SetKickBoard();
            yield return new WaitForSeconds(movingTime); // movingTime동안 이동
            kickboard.SetActive(false);
            kickboardSpeat.SetActive(false);
            kickboardLocated = false;
            yield return new WaitForSeconds(waitingTime); // waitingTime동안 대기
        }
    }

    IEnumerator Wait(float waitingtime)
    {
        yield return new WaitForSeconds(waitingtime);
        InitialSetting();
    }

    private void OnTriggerEnter(Collider other) {

        if (other.CompareTag("Obstacle")) {
            if (other.name == "dove_parent" || other.transform.parent.gameObject.name == "dove_parent")
            {
                StartCoroutine(Fly(8.0f, 8.0f));
            }  //충돌된 obstacle이 비둘기인지 확인하는 if문
            else
            {
                isStopping = true;
                StartCoroutine(Stop(3.0f, other.gameObject)); // Obstacle이랑 부딪히면, 3초간 멈춤
            }

        }
    }

}

    

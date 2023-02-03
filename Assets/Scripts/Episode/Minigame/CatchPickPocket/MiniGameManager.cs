using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Utility.Core;
using static Data.CustomEnum;
public class MiniGameManager : MonoBehaviour
{
    [Header("라우")]
    public CharacterManager rau;
    public float rauSpeed;
    public float acceleration;
    bool accTrigger = false;
    public bool conflict = false;
    public ParticleSystem particle;
    Vector3 forwardDir;

    [Header("PickPocket")]
    public GameObject pickPocket;
    public float pickPocketSpeed;

    [Header("Kickboard")]
    public GameObject kickboardParent;
    Vector3 kickboardDir;
    Vector3 kickboardMovVec;
    bool kickboardLocated = false;
    bool kickboardTrigger = false;
    int kickboardNum;
    float kickboardSpeed = 3.5f;

    [Header("Dove")]
    Vector3 flyDir = new Vector3(1, 1, 1);
    float flySpeed = 0.5f;
    public bool flyTrigger = false;
    bool conflictDove = false;

    [Header("boxes")]
    public GameObject boxParent;
    int boxGroup = 0;

    [Header("Timer")]
    public float timer;
    public Text textTimer;
    //public Text textItem;

    [Header("판정")]
    public float successDistance; // 라우~소매치기 거리가 어느정도 되야 성공하는지.

    public bool start = false;
    public bool begin = false;
    Coroutine initialCor;
    float tempSpeed;
    public GameObject peopleParent;
    int peopleGroup = 0; // 어떤 people그룹인지
    public bool isStopping = false;
    float initialRauSpeed;

    void Start() {
        //doves = doveParent.GetComponentInChildren<Transform>(); @비둘기 하나하나 사용할 때 쓰기
        //textTimer.gameObject.SetActive(false);
        //DataController.instance_DataController.isMapChanged = true;
        particle.Play();
        initialRauSpeed = rauSpeed;
        rau = DataController.instance.GetCharacter(Character.Rau);
    }

    void Update()
    {
        if (!start)
        {
            start = true;
            StartCoroutine(Wait(3.0f)); // 3초 후 시작.
        }
        else if (start && begin)
        {
            // 미니게임매니저 이동
            transform.position = rau.transform.position + new Vector3(0, 0.5f, 0);

            // 파티클 이동
            particle.transform.position = rau.transform.position + new Vector3(0, 1.5f, 10);

            // 라우 이동
            rau.ctrl.Move(new Vector3(0, 0, 1) * rauSpeed * Time.deltaTime);

            if (JoystickController.instance.inputDirection == new Vector2(0, 0)) // 조이스틱 인풋 없을 때 forwadDir방향 바라보게하기
            { 
                rau.gameObject.transform.eulerAngles = forwardDir;
            }

            // 멈췄을 때 조이스틱 input값 (0,0)으로 만들어서 못움직이게 하기
            /*if (isStopping) {
                DataController.instance_DataController.inputDirection = new Vector2(0, 0);
            }*/

            // 아이템 먹으면 일정시간 가속 이동
            if (accTrigger)
            {
                rauSpeed += acceleration * Time.deltaTime; // 라우 가속도 운동

            }

            // 소매치기 이동
            pickPocket.transform.Translate(new Vector3(0, 0, 1) * pickPocketSpeed * Time.deltaTime);

            if (kickboardLocated)
            {
                kickboardParent.transform.Translate(kickboardDir);
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
        begin = true;
        pickPocket.SetActive(true);
        //textTimer.gameObject.SetActive(true); // 타이머 보이게 하기
        SetDataController(); // 데이터 컨트롤러 정보 세팅
        SetPickPocket(); // 소매치기 위치 세팅
        SetKickBoard(); // 킥보드 위치 세팅
        StartCoroutine(KickboardCycle(4.0f, 8.0f)); // 킥보드 2초동안 움직임. 8초에 한번씩 킥보드 위치시킴.
    }

    private void SetDataController()
    {   // 카메라 설정
        DataController.instance.camInfo.camDis = new Vector3(0,1.3f,-2.5f);
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
        //kickboard.transform.position = rau.transform.position + new Vector3(kickboardNum * x, 0, z);
        kickboardParent.transform.position = rau.transform.position + new Vector3(kickboardNum * x, 0, z);

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
        //doveParent.transform.position = doveParent.transform.position + new Vector3(0, 0, 20); // 라우와 20 떨어진 위치에 위치시키기. (일단, 뭉탱이로)
    }

    private void SetPeople()
    {
        // peopleGroup값이 0이면, 이동시켜야할 people그룹이 "peopleParent"의 첫번째 자식 그룹.
        // peopleGroup값이 1이면, 이동시켜야할 people그룹이 "peopleParent"의 두번째 자식 그룹.
        Transform nextPeopleGroup = peopleParent.transform.GetChild(peopleGroup).gameObject.transform;
        nextPeopleGroup.position = nextPeopleGroup.position + new Vector3(0, 0, 30); // 20만큼 떨어진 곳에 위치시키기
        // 인덱스 변경
        if (peopleGroup == 0) peopleGroup = 1;
        else if (peopleGroup == 1) peopleGroup = 0;

    }

    private void SetBox()
    {
        // boxGroup값이 0이면, 이동시켜야할 box그룹이 "peopleParent"의 첫번째 자식 그룹.
        // boxGroup값이 1이면, 이동시켜야할 box그룹이 "peopleParent"의 두번째 자식 그룹.
        Transform nextBoxGroup = boxParent.transform.GetChild(boxGroup).gameObject.transform;
        nextBoxGroup.position = nextBoxGroup.position + new Vector3(0, 0, 85); // 85만큼 떨어진 곳에 위치시키기
        //인덱스 변경
        if (boxGroup == 0) boxGroup = 1;
        else if (boxGroup == 1) boxGroup = 0;
    }

    IEnumerator Stop(float time, GameObject other) // 충돌 시 멈춤.
    {
        if (isStopping)//isStopping이 참일 때만 활성화.
        {
            JoystickController.instance.joystick.gameObject.SetActive(false); // 조이스틱 없애기
            float tempAcceleration = acceleration;
            //float tempSpeed = rauSpeed;
            rauSpeed = 0;
            acceleration = 0;
            yield return new WaitForSeconds(time);
            JoystickController.instance.joystick.gameObject.SetActive(true); // 조이스틱 생기게
            rauSpeed = initialRauSpeed; // 장애물과 충돌하면, 라우 스피트는 다시 초기화!
            acceleration = tempAcceleration; // 장애물과 충돌하면, 라우 가속도 다시 초기화!
            //rau.isCollisionObstacle = false;
            isStopping = false;//장애물 사라지고 나서 거짓으로 초기화
        }
;
    }

    IEnumerator Fly(float movingTime, float waitingTime) // 비둘기 날아가는거
    {
        flyTrigger = true;
        yield return new WaitForSeconds(5.0f); // 5초동안 날아감.
        flyTrigger = false;
        yield return new WaitForSeconds(2.0f); // 2초동안 대기했다가 다음 위치로 배치됨.
        setDoves(); // 다음 위치로 배치됨.
    }

    IEnumerator KickboardCycle(float movingTime, float waitingTime)
    {
        while (true)
        {
            if (DataController.instance.mapCode != "010001") break;
            //kickboard.SetActive(true);
            //kickboardSpeat.SetActive(true);
            kickboardParent.SetActive(true);
            SetKickBoard();
            yield return new WaitForSeconds(movingTime); // movingTime동안 이동
            //kickboard.SetActive(false);
            //kickboardSpeat.SetActive(false);
            kickboardParent.SetActive(false);
            kickboardLocated = false;
            yield return new WaitForSeconds(waitingTime); // waitingTime동안 대기
        }
    }

    IEnumerator Wait(float waitingtime)
    {
        yield return new WaitForSeconds(waitingtime);
        forwardDir = rau.transform.position - DataController.instance.cam.transform.position;
        forwardDir = forwardDir.normalized;
        rau.gameObject.transform.eulerAngles = forwardDir;
        InitialSetting();
    }

    IEnumerator Accerlerate(float waitingtime)
    {
        yield return new WaitForSeconds(waitingtime);
        accTrigger = false;
        rauSpeed = initialRauSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Obstacle"))
        {
            if (other.name == "dove_parent" || other.transform.parent.gameObject.name == "dove_parent")
            {
                StartCoroutine(Fly(8.0f, 8.0f));
            }  //충돌된 obstacle이 비둘기인지 확인하는 if문
            else
            {
                isStopping = true;
                //rau.anim.SetBool("2DSide", false);
                StartCoroutine(Stop(3.0f, other.gameObject)); // Obstacle이랑 부딪히면, 3초간 멈춤
            }

        }
        // people 재활용 하기위해 사용됨
        else if (other.name == "SetPeopleCollider")
        {
            other.transform.position = other.transform.position + new Vector3(0, 0, 15); // 15 떨어진 위치로~
            SetPeople();
        }
        else if (other.name == "SetBoxesCollider")
        {
            other.transform.position = other.transform.position + new Vector3(0, 0, 40);
            SetBox();
        }
        else if (other.name == "item")
        {
            print("아이템 먹음");
            accTrigger = true;
            StartCoroutine(Accerlerate(5.0f)); // 5초간 가속
        }
        
    }

}

    

using System.Collections;
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
    bool accTrigger = false;
    public bool conflict = false;
    public ParticleSystem particle;

    [Header("PickPocket")]
    public GameObject pickPocket;
    public float pickPocketSpeed;

    [Header("킥보드")]
    public GameObject kickboardParent;
    Vector3 kickboardDir;
    Vector3 kickboardMovVec;
    bool kickboardLocated = false;
    bool kickboardTrigger = false;
    int kickboardNum;
    float kickboardSpeed = 5f;

    [Header("Dove")]
    Vector3 flyDir = new Vector3(1, 1, 1);
    float flySpeed = 0.5f;
    public bool flyTrigger = false;
    bool conflictDove = false;
    public GameObject doveParent;
    public List<Vector3> dovesPos = new List<Vector3>();

    [Header("Timer")]
    public float timer;
    public Text textTimer;
    //public Text textItem;

    [Header("판정")]
    public float successDistance; // 라우~소매치기 거리가 어느정도 되야 성공하는지.

    [Header("아이템")]
    //public GameObject itemParent; // 아이템 뭉탱이로 만들 경우 사용!
    //public List<Transform> items = new List<Transform>(); // 아이템 뭉탱이로 만들 경우 사용!
    public GameObject item1;
    public GameObject item2;

    bool start = false;
    bool begin = false;
    Coroutine initialCor;
    float tempSpeed;
    public GameObject obstacleParent;
    int ObstacleGroup = 0; // 어떤 people그룹인지
    public bool isStopping = false;
    float initialRauSpeed;

    void Start()
    {
        //textTimer.gameObject.SetActive(false);
        particle.Play();
        initialRauSpeed = rauSpeed;

        // 비둘기 리스트에 넣기
        for (int i = 0; i < doveParent.transform.childCount; i++)
        {
            dovesPos.Add(doveParent.transform.GetChild(i).transform.position);

        }

        /*
        // 아이템 뭉탱이로 만들어서 사용할 때! 아이템 리스트에 넣기. 
        for (int i = 0; i < itemParent.transform.childCount; i++)
        {
            items.Add(itemParent.transform.GetChild(i).transform);

        }*/

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
            particle.transform.position = rau.transform.position + new Vector3(0, 1.5f, 2);

            // 라우 이동
            rau.ctrl.Move(new Vector3(0, 0, 1) * rauSpeed * Time.deltaTime);

            if (isStopping) // 라우 멈출 때 슬라이딩? 하는거 방지 (완벽 X)
            {
                rau.moveHorDir = Vector3.zero;
                rau.moveVerDir = Vector3.zero;
                DataController.instance_DataController.inputDirection.x = 0;
                DataController.instance_DataController.inputDirection.y = 0;
            }


            /*
            // 아이템 뭉탱이로 만들어서 사용할 때! 아이템 회전 효과
            for (int i = 0; i < items.Count ; i++)
            {
                items[i].Rotate(0, 2 * Time.deltaTime, 0); // 아이템 회전 속도 2 
            }*/

            // 아이템 회전 효과
            item1.transform.Rotate(new Vector3(100, 100, 100) * Time.deltaTime);
            item2.transform.Rotate(new Vector3(-100, -100, -100) * Time.deltaTime);

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
        pickPocket.SetActive(true);
        //textTimer.gameObject.SetActive(true); // 타이머 보이게 하기
        SetDataController(); // 데이터 컨트롤러 정보 세팅
        SetPickPocket(); // 소매치기 위치 세팅
        SetKickBoard(); // 킥보드 위치 세팅
        StartCoroutine(KickboardCycle(4.0f, 8.0f)); // 킥보드 2초동안 움직임. 8초에 한번씩 킥보드 위치시킴.
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

    private void SetDoves()
    {
        doveParent.SetActive(true);

        for (int i = 0; i < doveParent.transform.childCount; i++)
        {
            //doveParent.transform.GetChild(i).transform.position = doveParent.transform.position;
            dovesPos[i] = dovesPos[i] + new Vector3(0, 0, 30);
            doveParent.transform.GetChild(i).transform.position = dovesPos[i];
        }

    }

    private void SetObstacles()
    {
        // peopleGroup값이 0이면, 이동시켜야할 people그룹이 "peopleParent"의 첫번째 자식 그룹.
        // peopleGroup값이 1이면, 이동시켜야할 people그룹이 "peopleParent"의 두번째 자식 그룹.
        Transform nextPeopleGroup = obstacleParent.transform.GetChild(ObstacleGroup).gameObject.transform;
        nextPeopleGroup.position = nextPeopleGroup.position + new Vector3(0, 0, 30); // 30만큼 떨어진 곳에 위치시키기
        // 인덱스 변경
        if (ObstacleGroup == 0)
        {
            ObstacleGroup = 1;
        }
        else if (ObstacleGroup == 1)
        {
            SetDoves();
            ObstacleGroup = 0;

        }

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
            particle.Play(); // 파티클 다시 플레이.
        }
;
    }

    IEnumerator Fly(float movingTime, float waitingTime) // 비둘기 날아가는거
    {
        flyTrigger = true;
        yield return new WaitForSeconds(3); // 3초동안 날아감.
        flyTrigger = false;
        doveParent.SetActive(false);
    }

    IEnumerator KickboardCycle(float movingTime, float waitingTime)
    {
        while (true)
        {
            if (DataController.instance_DataController.mapCode != "010001") break;
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
                particle.Pause(); // 파티클 멈추기
                StartCoroutine(Stop(3.0f, other.gameObject)); // Obstacle이랑 부딪히면, 3초간 멈춤
            }

        }
        // obstacles 재활용 하기위해 사용됨
        else if (other.name == "setObstaclesCollider")
        {
            print("부딪! 장애물 ");
            other.transform.position = other.transform.position + new Vector3(0, 0, 15); // 15 떨어진 위치로~
            SetObstacles();
        }
        else if (other.name == "item")
        {
            accTrigger = true;
            StartCoroutine(Accerlerate(5.0f)); // 5초간 가속
        }

    }

}

    

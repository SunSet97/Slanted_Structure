using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RunGameManager : MonoBehaviour
{
    [Header("스핏")]
    public CharacterManager speat;
    float originMoveVerDirY; // 스핏이 땅에 닿았을 때 최초의 speat.moveVerDir.y값 담기
    bool a = false; //스핏이 땅에 닿았을 때 최초의 speat.moveVerDir.y값을 얻기 위해 사용되는 bool 변수
    Animator anim; // 애니메이터
    Vector3 groundPos;

    [Header("적")]
    public MiniGameManager chaser;

    [Header("점프")]
    bool clickJump = false;
    bool isJump = false;

    [Header("클리어박스")]
    public CheckMapClear clearBox;

    [Header("스핏~적 처음 거리(m)")]
    public float distance;

    [Header("스피드")]
    public float speatSpeed;
    public float chaserSpeed;
    public float speedWithSmallObstacle = 3.0f; // 작은 장애물과 부딪혔을 때 distance에서 줄어야 하는 값.(== 작은 장애물과 부딪혔을 때 스핏의 속도 감속 크기)
    public float speedWithBigObstacle = 5.0f; // 큰 장애물과 부딪혔을 때 distance에서 줄어야 하는 값. (== 큰 장애물과 부딪혔을 때 스핏의 속도 감속 크기)
    float settedSpeatSpeed;

    [Header("타이머 설정")]
    public float timer;

    [Header("UI")]
    public Text timerText;
    public Text distanceText;
    public Text reStartText;

    [Header("프리팹 관련 넣을 부모 오브젝트")]
    public GameObject parent; // 프리팹들을 담을 오브젝트

    [Header("Renderer")]
    public Renderer speatRenderer;
    Color originColor;
    bool flickerTrigger = false;
    float flickerDegree; // 깜빡임 정도.

    [Header("Waypoints")]
    public Waypoint waypoints;


    Vector3 chaserPosition; // 적. 스핏 쫓는 애.
    public GameObject[] prefabList; // 여기에 
    GameObject one, two, three, end;
    GameObject recentReferVar; // 가장 최근에 충돌해서 업데이트된 참조변수 이름

    int patternType; // 패턴 종류
    int patternSubType; // 각 패턴마다 존재하는 프리팹 숫자
    int patternTypeNum = 3; // 패턴 몇개? 3개
    int patternSubTypeNum = 6; // 각 패턴마다 존재하는 총 프리팹 몇개? 6개

    public GameObject startPosition0; // 콜라이더랑 충돌 후 다시 배치되는 위치
    public GameObject startPosition1; // one의 처음 위치
    public GameObject startPosition2; // two의 처음위치
    public GameObject startPosition3; // three의 처음위치

    bool isStopMapPrefab = false; // true면 성공. true된 즉시 end 프리팹 보이게!
    bool clickSkill = false;
    bool inConflict = false; // false면 충돌 안하고 있는 상태. true면 충돌하고 있는 상태.
    bool isGameOver = false;

    Coroutine slowEffectCoroutine;
    RunGameManager runGameManagerInstance;
    BoxCollider boxColliderInstance;

    void Start()
    {

        if (transform.name == "RunGameManager") // 장애물 충돌 이외 모든 것을 RunGameManger에서 관리
        {
            // 카메라무빙없애기
            //DataController.instance_DataController.cam.GetComponent<Camera_Moving>().enabled = false;

            //DataController.instance_DataController.isMapChanged = true;

            // 캐릭터
            speat = DataController.instance_DataController.currentChar;

            // 애니메이터
            anim = speat.GetComponent<Animator>();

            // 런게임 진행하는 동안 조이스틱 안쓰이니깐 안보이게 하기
            //DataController.instance_DataController.joyStick.gameObject.SetActive(false);

            // DataController 카메라 설정값 변경
            DataController.instance_DataController.camDis.x = 4f;
            DataController.instance_DataController.camDis.y = 3.5f;

            // 카메라 뷰 설정
            DataController.instance_DataController.cam.orthographicSize = 4;
            DataController.instance_DataController.cam.orthographic = true;

            LoadPattern(); // start, end를 제외한 나머지 패턴 프리팹 로딩.
            SetPrefab(); // prefabList 배열에서 랜덤하게 3개 선택.
            end = Instantiate(Resources.Load<GameObject>("Run_Pattern/End"));
            end.transform.SetParent(parent.transform, false);
            end.SetActive(false);

            startPosition0.SetActive(false);
            startPosition1.SetActive(false);
            startPosition2.SetActive(false);
            startPosition3.SetActive(false);

            // 스핏의 컬러를 originColor에 담는다.
            print("speatRenderer.material.color: " + transform.name + "의 " + speatRenderer.material.color);
            originColor = speatRenderer.material.color;

            //chaserPosition = speat.transform.position + new Vector3(distance, 0, 0); // 적이 스핏과 distance(x좌표 기준)만큼 떨어져있다.
        }
        else if (transform.name == "ObstacleCollider") // ObastacleCollider는 오직 장애물 충돌만 관여하도록!!
        {
            runGameManagerInstance = transform.parent.GetComponent<RunGameManager>();
            boxColliderInstance = transform.GetComponent<BoxCollider>();
            settedSpeatSpeed = runGameManagerInstance.speatSpeed;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (transform.name == "RunGameManager")
        {
            if (!speat)
            {
                if (DataController.instance_DataController.cam.GetComponent<Camera_Moving>().enabled)
                {
                    DataController.instance_DataController.cam.GetComponent<Camera_Moving>().enabled = false;
                }
            }
            


            // 깜빡거림
            if (flickerTrigger)
            {
                flickerDegree = Mathf.Abs(Mathf.Sin(Time.time * 10));
                speatRenderer.material.color = originColor * flickerDegree;
            }

            // 타이머
            timer -= Time.deltaTime;
            timerText.text = "런게임 타이머: " + Mathf.Round(timer);

            // 거리
            distanceText.text = "런게임 거리: " + Mathf.Round(distance) + "m";

            // 점프할 때 사용할 originMoveVerDirY 값 설정 
            if (!a && speat.ctrl.isGrounded)
            {
                a = true;
                groundPos = speat.transform.position;
                originMoveVerDirY = speat.moveVerDir.y;
            }

            // 점프
            if (DataController.instance_DataController.inputJump && anim.GetBool("Jump"))
            {
                anim.SetBool("Jump", false); //점프 불가능 상태로 변경하여 연속적인 점프 제한
                speat.moveVerDir.y = originMoveVerDirY;
                speat.jumpForce = 15; // 내가 임의로 설정!
                speat.gravityScale = 3.0f;// 디폴트는 1.1f이었음.

            }

            if (clickJump && speat.transform.position.y >= groundPos.y + 0.5f) // 스핏의 y좌표값이 0.5값보다 같거나 위에 있을 때 '점프중'으로 간주.
            {
                clickJump = false;
                isJump = true;
            }

            if (isJump && speat.ctrl.isGrounded) // 점프했다가 내려온거!
            {
                isJump = false;
                DataController.instance_DataController.inputJump = false;
            }

            // 프리팹 이동 관련
            if (!isStopMapPrefab)
            {
                one.transform.Translate(new Vector3(speatSpeed * Time.deltaTime, 0, 0));
                two.transform.Translate(new Vector3(speatSpeed * Time.deltaTime, 0, 0));
                three.transform.Translate(new Vector3(speatSpeed * Time.deltaTime, 0, 0));
            }

            // 성공
            if (Mathf.Round(timer) == 0 && !isGameOver)
            {
                print("성공");
                isStopMapPrefab = true; // 성공했으니 End 프리팹으로 ㄱㄱㄹ해야되는대 아직 안함!ㅎ
                SetEndPrefab();
            }

            // 실패
            if (Mathf.Round(distance) <= 0 && !isGameOver)
            {
                isGameOver = true;
                isStopMapPrefab = true; // 프리팹 이동 멈추게 하기.
                print("적에게 잡힘. 게임오버.");
                //restartCoroutine = StartCoroutine(ReStartGame(3));
                StartCoroutine(ReStartGame());

            }

        }
        else if (transform.name == "ObstacleCollider")
        {
            transform.position = speat.transform.position + new Vector3(0, 0.5f, 0);

        }

    }

    // 게임 시작할 때 사용되며, 새로운 패턴 종류 지정하고 prefabList에 담는다.
    void LoadPattern()
    {
        patternType = Random.Range(0, patternTypeNum); // 세가지 패턴 중 랜덤으로 선택.
        prefabList = Resources.LoadAll<GameObject>("Run_Pattern/Pattern" + patternType);

        for (int i = 0; i < prefabList.Length; i++)
        {
            prefabList[i] = Instantiate(prefabList[i]);
            prefabList[i].transform.SetParent(parent.transform, false);
            prefabList[i].SetActive(false);
        }

    }

    // 맨 처음 3개 프리팹(one, two, three) 선택.
    private void SetPrefab()
    {
        // 중복없이 난수 생성. one은 start랑 동일(초반세팅이깐). randomArr[1]은 two의 patternSubType. randomArr[2]은 three의 patternSubType.
        int[] randomArr = GetRandomInt(2, 0, patternSubTypeNum);

        // 게임 처음 시작할 때 one은 Start 객체를 가리키도록!
        one = Instantiate(Resources.Load<GameObject>("Run_Pattern/Start"));
        one.transform.SetParent(parent.transform, false);
        one.SetActive(true);
        one.transform.position = startPosition1.transform.position;

        two = prefabList[randomArr[0]];
        two.SetActive(true);
        two.transform.position = startPosition2.transform.position;

        three = prefabList[randomArr[1]];
        three.SetActive(true);
        three.transform.position = startPosition3.transform.position;

    }

    private void SetPrefab(GameObject obj)
    { // 파라미터는 충돌한 게임 오브젝트를 받음. 일단 랜덤으로.
        while (true)
        {
            patternSubType = Random.Range(0, patternSubTypeNum);

            if (prefabList[patternSubType] == one || prefabList[patternSubType] == two || prefabList[patternSubType] == three)
            {
                continue;
            }
            else
            {
                if (obj == one)
                {
                    one.SetActive(false);
                    one = prefabList[patternSubType];
                    one.SetActive(true);
                    one.transform.position = startPosition0.transform.position;
                    recentReferVar = one;
                }
                // 충돌된 오브젝트가 two일 때
                else if (obj == two)
                {
                    two.SetActive(false);
                    two = prefabList[patternSubType];
                    two.SetActive(true);
                    two.transform.position = startPosition0.transform.position;
                    recentReferVar = two;
                }
                // 충돌된 오브젝트가 three일 때
                else if (obj == three)
                {
                    three.SetActive(false);
                    three = prefabList[patternSubType];
                    three.SetActive(true);
                    three.transform.position = startPosition0.transform.position;
                    recentReferVar = three;
                }

                break;

            }

        }

    }

    private void SetEndPrefab()
    {

        // 맨홀쪽으로 이동해야하니깐, 조이스틱 다시 사용할 수 있게 하기.
        DataController.instance_DataController.joyStick.gameObject.SetActive(true);

        // 웨이포인트 사용
        waypoints.transform.GetChild(0).transform.position = speat.transform.position; // 첫번째 waypoint(1)을 스핏 위치와 일치 시키기
        waypoints.transform.GetChild(1).transform.position = waypoints.transform.GetChild(0).transform.position + new Vector3(10, 0 , 0); // waypoint(1)보다 x축으로 10떨어진 곳에 waypoint(2)를 위치시킨다.
        waypoints.transform.GetChild(2).transform.position = waypoints.transform.GetChild(1).transform.position + new Vector3(0, 0, 10); // waypoint(2)보다 x축으로 10떨어진 곳에 waypoint(3)를 위치시킨다.

        end.transform.position = recentReferVar.transform.position + new Vector3(23.0f, 0, -8.0f);
        end.SetActive(true);
        clearBox.transform.position = end.transform.position + new Vector3(2.0f, 0, -13.0f);



    }

    private void flicker()
    {
        flickerTrigger = true;
    }

    // 점프버튼 누르면 실행되는 함수
    public void JumpBtn()
    {
        if (transform.name == "RunGameManager")
        { // RunGamManager 오브젝트에서만 실행되도록
            if (speat.ctrl.isGrounded)
            {
                clickJump = true;
                DataController.instance_DataController.inputJump = true;
                anim.SetBool("Jump", true);
            }
        }

    }

    // 스킬버튼 누르면 실행되는 함수
    public void SkillBtn()
    {
        if (transform.name == "ObstacleCollider") // ObstacleCollider 오브젝트에서만 실행되도록
        {
            StartCoroutine(Skill(1.0f));
        }
    }

    private void OnTriggerEnter(Collider other) // *!공부! 이거 반응하려면 rigidbody 컴포넌트 추가해야돼
    {
        // 프리팹 변경할 때 쓰임
        if (transform.name == "RunGameManager")
        {
            SetPrefab(other.gameObject.transform.parent.parent.gameObject);
        }

        // 장애물 충돌 여부 확인할 때 쓰임.
        if (transform.name == "ObstacleCollider" && other.CompareTag("Obstacle") && !inConflict)
        {
            StartCoroutine(Flicker(1.0f));
            inConflict = true;
            if (other.transform.parent.name == "Small")
            {
                print("스몰 충돌!");
                runGameManagerInstance.distance -= speedWithSmallObstacle;
                StartCoroutine(SlowDownSpeed(1.0f, settedSpeatSpeed - speedWithSmallObstacle)); // 1.0f초동안 스핏의 속도는 5.0m/s로
                //print("★conflict with Small && speatSpeed is: " + runGameManagerInstance.speatSpeed);

            }
            else if (other.transform.parent.name == "Big")
            {
                print("빅 충돌!");
                runGameManagerInstance.distance -= speedWithBigObstacle;
                StartCoroutine(SlowDownSpeed(1.0f, settedSpeatSpeed - speedWithBigObstacle)); // 1.0f초동안 스핏의 속도는 7.0m/s로
                //print("♥conflict with Big && speatSpeed is: " + runGameManagerInstance.speatSpeed);

            }
        }
    }

    IEnumerator SlowDownSpeed(float waitTime, float reducedSpeed)
    { // waitTime동안 reduceSpeed로 스핏 속도 줆.

        runGameManagerInstance.speatSpeed = reducedSpeed;
        yield return new WaitForSeconds(waitTime);
        // ☆ StopCoroutine(slowEffectCoroutine);
        runGameManagerInstance.speatSpeed = settedSpeatSpeed;
        inConflict = false;
    }

    IEnumerator Skill(float waitTime)
    { // waitTime동안 skill사용(콜라이더 없앴다가 나타나게)
        boxColliderInstance.enabled = false;
        yield return new WaitForSeconds(waitTime);
        boxColliderInstance.enabled = true;
    }

    IEnumerator ReStartGame()
    {
        reStartText.gameObject.SetActive(true);

        reStartText.text = "3";
        yield return new WaitForSeconds(1);

        reStartText.text = "2";
        yield return new WaitForSeconds(1);

        reStartText.text = "1";
        yield return new WaitForSeconds(1);

        reStartText.text = "Start!";
        yield return new WaitForSeconds(1);

        reStartText.gameObject.SetActive(false);

        timer = 60.0f; // 다시 타이머 60초로 설정.
        distance = 25; // 다시 distacne를 25로 설정.
        one.SetActive(false);
        two.SetActive(false);
        three.SetActive(false);
        SetPrefab(); // 다시 프리팹 설정
        isStopMapPrefab = false; // 다시 프리팹 움직이게 ㄱㄱ
        isGameOver = false;

    }

    IEnumerator Flicker(float waitingTime) // 깜빡깜빡 거리는거
    { // waitTime동안 투명<->반투명 왔다리 갔다리
        runGameManagerInstance.flickerTrigger = true;
        yield return new WaitForSeconds(waitingTime);
        runGameManagerInstance.speatRenderer.material.color = runGameManagerInstance.originColor; // 다시 본래 Material로!
        runGameManagerInstance.flickerTrigger = false;

    }

    // 기타 함수들.
    // 중복없는 난수 배열 받는 함수.
    private int[] GetRandomInt(int length, int min, int max)
    {
        int[] randomArr = new int[length];
        bool isSame = false;

        for (int i = 0; i < length; i++)
        {
            while (true)
            {
                randomArr[i] = Random.Range(min, max);
                isSame = false;
                for (int j = 0; j < i; ++j)
                {
                    if (randomArr[i] == randomArr[j])
                    {
                        isSame = true;
                        break;
                    }
                }
                if (!isSame) break;
            }
        }

        return randomArr;

    }

}

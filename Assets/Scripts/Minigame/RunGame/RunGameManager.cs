﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RunGameManager : MonoBehaviour
{    
    [Header("스핏")]
    public CharacterManager speat;
    
    [Header("적")]
    public MiniGameManager chaser;

    [Header("점프")]
    public float jumpPower;
    public float gravity;
    bool clickJump = false;

    [Header("클리어박스")]
    public CheckMapClear clearBox;

    [Header("스핏~적 처음 거리(m)")]
    public float distance;

    [Header("스피드")]
    public float speatSpeed;
    public float chaserSpeed;
    public float speedWithSmallObstacle = 3.0f; // 작은 장애물과 부딪혔을 때 스핏의 속도 감속 크기 => 즉, -3m/s가되어 스핏의 속도는 7m/s
    public float speedWithBigObstacle = 5.0f; // 큰 장애물과 부딪혔을 때 스핏의 속도 감속 크기 => 즉, -5m/s가되어 스핏의 속도는 5m/s
    float settedSpeatSpeed;

    [Header("타이머 설정")]
    public float timer;

    [Header("UI")]
    public Text timerText;
    public Text distanceText;

    [Header("프리팹 관련 넣을 부모 오브젝트")]
    public GameObject parent; // 프리팹들을 담을 오브젝트

    Vector3 chaserPosition; // 적. 스핏 쫓는 애.
    GameObject[] prefabList; // 여기에 
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

    bool success = false; // true면 성공. true된 즉시 end 프리팹 보이게!
    bool clickSkill = false;
    bool inConflict = false; // false면 충돌 안하고 있는 상태. true면 충돌하고 있는 상태.

    RunGameManager runGameManagerInstance;
    BoxCollider boxColliderInstance;
    //public Renderer renderer;

    void Start() {

        if (transform.name == "RunGameManager")
        {
            // DataController 카메라 설정값 변경
            DataController.instance_DataController.camDis_x = 4f;
            DataController.instance_DataController.camDis_y = 3.3f;

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

            //chaserPosition = speat.transform.position + new Vector3(distance, 0, 0); // 적이 스핏과 distance(x좌표 기준)만큼 덜어져있다.
        }
        else if (transform.name == "ObstacleCollider")
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
            // 타이머
            timer -= Time.deltaTime;
            timerText.text = "런게임 타이머: " + Mathf.Round(timer);

            // 거리
            distanceText.text = "런게임 거리: " + Mathf.Round(distance) + "m";

            // 점프
            if (clickJump)
            {
                clickJump = false;
                speat.isJump = true;
                speat.jumpForce = 10; // 내가 임의로 설정!
            }

            // 프리팹 이동 관련
            if (!success)
            {
                one.transform.Translate(new Vector3(speatSpeed * Time.deltaTime, 0, 0));
                two.transform.Translate(new Vector3(speatSpeed * Time.deltaTime, 0, 0));
                three.transform.Translate(new Vector3(speatSpeed * Time.deltaTime, 0, 0));
            }
            
            // 성공
            if (Mathf.Round(timer) == 0)
            {
                print("성공");
                success = true; // 성공했으니 End 프리팹으로 ㄱㄱㄹ
                SetEndPrefab();
            }

            // 실패
            if (Mathf.Round(distance) <= 0)
            {
                print("적에게 잡힘. 게임종료.");
                timer = 60.0f;
            }

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

    private void SetEndPrefab() {

        end.transform.position = recentReferVar.transform.position + new Vector3(23.0f, 0, -8.0f);
        end.SetActive(true);
        clearBox.transform.position = end.transform.position + new Vector3(2.0f,0,-13.0f);

    }

    // 점프버튼 누르면 실행되는 함수
    public void JumpBtn() {

        if (transform.name == "RunGameManager") { // RunGamManager 오브젝트에서만 실행되도록
            if (speat.ctrl.isGrounded)
            {
                clickJump = true;
            }
        }

    }

    // 스킬버튼 누르면 실행되는 함수
    public void SkillBtn() {
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
            inConflict = true;
            if (other.transform.parent.name == "Small")
            {
                runGameManagerInstance.distance -= speedWithSmallObstacle;
                StartCoroutine(SlowDownSpeed(1.0f, settedSpeatSpeed - speedWithSmallObstacle)); // 1.0f초동안 스핏의 속도는 5.0m/s로
            }
            else if (other.transform.parent.name == "Big")
            {
                runGameManagerInstance.distance -= speedWithBigObstacle;
                StartCoroutine(SlowDownSpeed(1.0f, settedSpeatSpeed - speedWithBigObstacle)); // 1.0f초동안 스핏의 속도는 7.0m/s로
            }
        }
    }

    IEnumerator SlowDownSpeed(float waitTime, float reducedSpeed) { // waitTime동안 reduceSpeed로 스핏 속도 줆.
        runGameManagerInstance.speatSpeed = reducedSpeed;
        //Coroutine slowEffectCoroutine = StartCoroutine(SlowEffect(0.2f)); // 0.2초 주기로 깜빡깜빡(투명 불투명 왔다리 갔다리)효과!
        yield return new WaitForSeconds(waitTime);
        //StopCoroutine(slowEffectCoroutine);
        runGameManagerInstance.speatSpeed = settedSpeatSpeed;
        inConflict = false;
    }

    IEnumerator Skill(float waitTime) { // waitTime동안 skill사용(콜라이더 없앴다가 나타나게)
        boxColliderInstance.enabled = false;
        yield return new WaitForSeconds(waitTime);
        boxColliderInstance.enabled = true;
    }

    // 쓸지 말지 고민좀..
    /*IEnumerator SlowEffect(float waitTime) { // waitTime동안 투명<->반투명 왔다리 갔다리
        renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b,0.5f); 
        yield return new WaitForSeconds(5.0f);
        renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, 1.0f);
    }*/

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

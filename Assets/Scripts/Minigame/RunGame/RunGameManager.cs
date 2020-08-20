using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RunGameManager : MonoBehaviour
{
    [Header("타이머 설정")]
    public float timer;

    [Header("UI관련 - 타이머 표시하는것만 있음")]
    public Text timerText;

    [Header("프리팹 관련 넣을 부모 오브젝트")]
    public GameObject parent; // 프리팹들을 담을 오브젝트

    //public GameObject speat;

    GameObject[] prefabList;
    GameObject one, two, three, end;
    int patternType; // 패턴 종류
    int patternSubType; // 각 패턴마다 존재하는 프리팹 숫자
    int patternTypeNum = 3; // 패턴 몇개? 3개
    int patternSubTypeNum = 4; // 각 패턴마다 존재하는 총 프리팹 몇개? 4개
    public GameObject startPosition; // = new Vector3(500, 500, 2000); // 임의로 설정
    Vector3 rePosition = Vector3.zero;
    //Vector3 startPosition1 = new Vector3(480, 500, 2000); // 임의로 설정
    //Vector3 startPosition2 = new Vector3(460, 500, 2000); // 임의로 설정
    //Vector3 startPosition3 = new Vector3(440, 500, 2000); // 임의로 설정
    //float goalDistance;
    float speatSpeed = 1.0f;
    float chaserSpeed = 0.7f;

    bool startTHeGame = false;

    void Start() {

        startTHeGame = true;
        /*start = Instantiate(Resources.Load<GameObject>("Run_Pattern/Start"));
        start.transform.SetParent(parent.transform);*/

        LoadPattern(); // start, end를 제외한 나머지 패턴 프리팹 로딩.
        SetPrefab(); // prefabList 배열에서 랜덤하게 3개 선택.

        end = Instantiate(Resources.Load<GameObject>("Run_Pattern/End"));
        end.transform.SetParent(parent.transform);
        end.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {   
        // 타이머
        timer -= Time.deltaTime;
        timerText.text = "런게임 타이머: " + Mathf.Round(timer);

        // 프리팹 이동 관련
        //start.transform.Translate(new Vector3(speatSpeed * Time.deltaTime, 0, 0));
        one.transform.Translate(new Vector3(speatSpeed * Time.deltaTime, 0, 0));
        two.transform.Translate(new Vector3(speatSpeed * Time.deltaTime, 0, 0));
        three.transform.Translate(new Vector3(speatSpeed * Time.deltaTime, 0, 0));
        end.transform.Translate(new Vector3(speatSpeed * Time.deltaTime, 0, 0));

    }

    // 게임 시작할 때 사용되며, 새로운 패턴 종류 지정하고 prefabList에 담는다.
    void LoadPattern()
    {
        //start.transform.SetParent(parent.transform); // Start에 있어도 무관할 듯?

        patternType = Random.Range(0, patternTypeNum); // 세가지 패턴 중 랜덤으로 선택.

        prefabList = Resources.LoadAll<GameObject>("Run_Pattern/Pattern" + patternType);

        for (int i = 0; i < prefabList.Length; i++)
        {
            prefabList[i] = Instantiate(prefabList[i]);
            prefabList[i].transform.SetParent(parent.transform);
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
        one.transform.SetParent(parent.transform);
        one.SetActive(true);
        one.transform.position = startPosition.transform.position;

        two = prefabList[randomArr[0]];
        two.SetActive(true);
        two.transform.position = startPosition.transform.position + new Vector3(-10,0,0);

        three = prefabList[randomArr[1]];
        three.SetActive(true);
        three.transform.position = startPosition.transform.position + new Vector3(-20, 0, 0);

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
                    one.transform.position = rePosition;
                    print("옹");
                }
                // 충돌된 오브젝트가 two일 때
                else if (obj == two)
                {
                    two.SetActive(false);
                    two = prefabList[patternSubType];
                    two.SetActive(true);
                    two.transform.position = rePosition;

                }
                // 충돌된 오브젝트가 three일 때
                else if (obj == three)
                {
                    three.SetActive(false);
                    three = prefabList[patternSubType];
                    three.SetActive(true);
                    three.transform.position = rePosition;

                }
               
                break;

            }

        }

    }

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

    private void OnTriggerEnter(Collider other) // *!공부! 이거 반응하려면 rigidbody 컴포넌트 추가해야돼
    {
        SetPrefab(other.gameObject.transform.parent.parent.gameObject);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("라우")]
    public CharacterManager rau;
    Vector3 rauPos;
    public float rauSpeed;
    public float acceleration;
    public float setStopTime;
    float settedStopTime;
    float settedRauSpeed;
    public bool conflict;
    public ParticleSystem ps;
    
    [Header("PickPocket")]
    public GameObject PP;
    public float ppSpeed;
    public float distanceBtwRauAndPP; // 게임 시작할 때 라우와 소매치기가 떨어진 거리.

    [Header("Bicycle")]
    public GameObject bicycle;
    Vector3 bicPos;
    public float setAppearDistance; // 라우와 어느정도 떨어져서 나타날 것인지
    public float bicycleSpeed;
    Vector3 moveDirection;
    public int appearTime; // 몇초에 한번 나타나기 시작할 것인지
    public bool bicycleSettingComplete; //true면 자전거 배치됨
    bool escapeToLeft; // true면 라우를 왼쪽으로 빗겨나가게 이동. false면 오른쪽으로 빗겨나가게 이동
    int typeNum; // bicycle의 등장 타입 결정하는 변수

    [Header("Timer")]
    public float timer;
    public Text textTimer;
    public Text textItem;

    [Header("판정")]
    public float completeDistance;
    bool gameover;

    [Header("UI")]
    public GameObject failPanel;
    public GameObject completePanel;

    void Start() {
        conflict = false;
        gameover = false;
        bicycleSettingComplete = false;
        escapeToLeft = true;
        bicycle.SetActive(false);
        rauPos = rau.transform.position;
        bicPos = bicycle.transform.position;
        PP.transform.position = rauPos + Vector3.forward * distanceBtwRauAndPP;
        ps.Play();
    }

    void Update()
    {
        // 타이머
        timer -= Time.deltaTime;
        textTimer.text = "남은 시간: " + Mathf.Round(timer);
        //textItem.text = "아이템 개수: " + rau.itemCount; // 아이템 일단 보류.

        // 라우 z방향으로 움직임. 충돌있으면 3초간 멈춤.
        //StartCoroutine(Move());
        if (!conflict)
        {
            rau.transform.Translate(0, 0, rauSpeed * Time.deltaTime);
            rauSpeed += acceleration * Time.deltaTime; // 등가속운동
        }
        else
        {
            StartCoroutine(Stop());
        }

        // 소매치기 잡기 실패.
        if (Mathf.Round(timer) == 0)
        {
            failPanel.SetActive(true);
            Time.timeScale = 0;
        }
        
        //소매치기 잡기 성공.
        if ((int)PP.transform.position.z - (int)rau.transform.position.z == completeDistance) {
            completePanel.SetActive(true);
            Time.timeScale = 0;
        }

        // 소매치기 이동
        PP.transform.Translate(Vector3.forward * ppSpeed * Time.deltaTime); //

        // 바이클 작동시키기
        if (bicycleSettingComplete == false && Mathf.Round(timer) % appearTime == 0) {
            Debug.Log("겜메니져. 바이클 active하기");
            bicycle.gameObject.SetActive(true); // bicycle 작동시키기
            DetermineType(); // bicycle 배치
        }
        // 바이클 움직이기
        transform.Translate(moveDirection * bicycleSpeed * Time.deltaTime);

        //bicycle이 화면밖으로 사라지면 setActive(false)
        if (rauPos.z - bicPos.z >= 5)
        {
            bicycleSettingComplete = false;
            escapeToLeft = escapeToLeft ? false : true;
            gameObject.SetActive(false);
        }
        
        //라우 가속. 그리고  충돌시 멈춤.
        settedStopTime = setStopTime;
        settedRauSpeed = rauSpeed;

        //
        ps.transform.position = rau.transform.position + new Vector3(0, 1.5f, 10); ;

        if (conflict)
        {
            ps.Stop();
        }
        else if (!conflict)
        {
            ps.Play();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.CompareTag("Obstacle"))
        {

            conflict = true;

        }
       
    }


    // DetermineType() : bicycle 출현 위치 선정
    void DetermineType()
    {
        typeNum = Random.Range(1, 4);
        switch (typeNum)
        {

            case 1: // 라우의 앞에서 등장
                bicycleSettingComplete = true; // 배치 완료 
                bicPos = new Vector3(rauPos.x, rauPos.y, rauPos.z + setAppearDistance);
                moveDirection = (bicPos - rauPos).normalized;
                break;

            case 2: // 라우의 오른쪽에서 등장  rau.transform.position.y
                bicycleSettingComplete = true; // 배치 완료 
                bicPos = new Vector3(rauPos.x + setAppearDistance, rauPos.y, rauPos.z + setAppearDistance);
                moveDirection = (bicPos - new Vector3(rauPos.x, bicPos.y, bicPos.z)).normalized;
                break;

            case 3: // 라우의 왼쪽에서 등장
                bicycleSettingComplete = true; // 배치 완료 
                bicPos = new Vector3(rauPos.x - setAppearDistance, rauPos.y, rauPos.z + setAppearDistance);
                moveDirection = (bicPos - new Vector3(rauPos.x, bicPos.y, bicPos.z)).normalized;
                break;
        }
    }


    IEnumerator Move()
    {
        // 라우 z방향으로 이동
        if (!conflict)
        {
            rau.transform.Translate(0, 0, rauSpeed * Time.deltaTime);
            rauSpeed += acceleration * Time.deltaTime; // 등가속운동
        }
        else
        {
            yield return new WaitForSeconds(setStopTime);
            conflict = false;
        }

    }

    IEnumerator Stop() {
        yield return new WaitForSeconds(setStopTime);
        conflict = false;
    }

    public void SceneTestClickListener()
    {
        SceneManager.LoadScene("StartScene");
    }
}

    

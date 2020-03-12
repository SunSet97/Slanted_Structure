using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Bicycle_phw : MonoBehaviour
{
    public Rau_phw rau;
    public GameManager_phw manager;
    float posY;
    public float setappearDistance; // 라우와 어느정도 떨어져 나타날 것인지
    public float moveSpeed;
    Vector3 moveDirection;
    public int appearTime; // 몇초에 한번 나타나기 시작할 것인지
    public bool bicycleSettingComplete; //true면 자전거 배치됨
    bool escapeToLeft; // true면 라우를 왼쪽으로 빗겨나가게 이동. false면 오른쪽으로 빗겨나가게 이동

    int i;
    int j;
    int appearType;


    void determineType() {

        i = Random.Range(1, 4);

        switch (i) {

            case 1: // 라우 앞에서 등장
                bicycleSettingComplete = true; // 배치 완료 
                transform.position = new Vector3(rau.transform.position.x,posY, rau.transform.position.z + setappearDistance);
                moveDirection = (transform.position - rau.transform.position).normalized;
                Debug.Log("앞에서 등장");
                break;

            case 2: // 라우 오른쪽에서 등장  rau.transform.position.y
                bicycleSettingComplete = true; // 배치 완료 
                transform.position = new Vector3(rau.transform.position.x + setappearDistance, posY, rau.transform.position.z + setappearDistance);
                moveDirection = (transform.position - new Vector3(rau.transform.position.x, transform.position.y, transform.position.z)).normalized;

                Debug.Log("오른쪽에서 등장");
                break;

            case 3: // 라우 왼쪽에서 등장
                bicycleSettingComplete = true; // 배치 완료 
                transform.position = new Vector3(rau.transform.position.x - setappearDistance, posY, rau.transform.position.z + setappearDistance);
                moveDirection = (transform.position - new Vector3(rau.transform.position.x, transform.position.y, transform.position.z)).normalized;

                Debug.Log("왼쪽에서 등장");
                break;

               }

        //moveDirection = (transform.position - new Vector3(rau.transform.position.x, transform.position.y, transform.position.z)).normalized;



    }



    void Start()
    {
        bicycleSettingComplete = false;
        escapeToLeft = true;
        gameObject.SetActive(false);
        posY = rau.transform.position.y;
    }
    



    void Update()
    {

        // apperTime에 한번 나타남. 자전거 배치
        if (bicycleSettingComplete == false && Mathf.Round(manager.timer) % appearTime == 0)
        {
            determineType();
            
        }

        transform.Translate(moveDirection * rau.moveSpeed * Time.deltaTime);

 
        /*
        // 앞방향 관련
        if (escapeToLeft)
        {
            transform.Translate(1.0f * Time.deltaTime, 0, moveSpeed * Time.deltaTime); // 라우를 향해 직진. 라우를 살짝 왼쪽으로 빗겨 나감. float값만큼.
        }
        else {
            transform.Translate(-1.0f * Time.deltaTime, 0, moveSpeed * Time.deltaTime); // 라우를 향해 직진. 라우를 살짝 오른쪽으로 빗겨 나감.
        }
           */

        // 화면밖으로 사라지면 자전거 장애물 사라짐
        if ((int)(rau.transform.position.z - transform.position.z) == 5) {
            bicycleSettingComplete = false;
            escapeToLeft = escapeToLeft ? false : true;
            gameObject.SetActive(false);
         }
        
    }
    
}

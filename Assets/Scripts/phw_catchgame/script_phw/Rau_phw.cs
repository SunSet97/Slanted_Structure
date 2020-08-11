using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rau_phw : MonoBehaviour
{
    //public Pigeon pigeon;
    public Rigidbody rigid;
    GameManager_phw manager;
    public float moveSpeed;
    public float acceleration; // 가속도
    float settedMoveSpeed;
    public float setStopTime; // 라우가 넘어지는 시간 설정
    float settedStopTime;
    public int itemCount;
    public bool conflictWithCrowd;
    public float jumpPower;
    bool isJump;
    //bool isStop;

    // about camera
    float screenLeft;
    float screenRight;
    public bool detectAround;


    IEnumerator move(bool conflict)
    {

        if (!conflict)
        {
            //직진
            transform.Translate(0, 0, moveSpeed * Time.deltaTime);

            //좌우이동
            if (Input.GetButton("Horizontal"))
                transform.Translate(moveSpeed * Input.GetAxisRaw("Horizontal") * Time.deltaTime, 0, 0);

            // 등가속운동
            moveSpeed += acceleration * Time.deltaTime;
        }
        else
        {
            //isStop = true;
            yield return new WaitForSeconds(setStopTime);
            conflictWithCrowd = false;
            //isStop = false;
        }
    }

    void Start()
    {

        settedStopTime = setStopTime;
        settedMoveSpeed = moveSpeed;
        conflictWithCrowd = false;
        itemCount = 0;
        isJump = false;

        // 라우가 카메라 밖을 벗어나지 못하게 하기
        /*screenLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
        screenRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;*/

    }

    void Update()
    {
        StartCoroutine(move(conflictWithCrowd));

        if (Input.GetKeyDown(KeyCode.Space) && isJump == false)
        {
            isJump = true;
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }

    }

    // 화면 밖으로 못나가게 하기! 아직 안쓰임. main이 좀 문제;
    /*void CheckOutOfScreen() {
        float nextX = Mathf.Clamp(transform.position.x, screenLeft, screenRight);
        transform.position = new Vector3(nextX, transform.position.y, transform.position.z);
    }*/

    void OnCollisionEnter(Collision collision)
    {

        //연속 점프 방지
        if (collision.gameObject.tag == "floor")
        {
            isJump = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
       
            // 아이템
            if (other.tag == "item")
            {
                other.gameObject.SetActive(false);
                itemCount++;
                Debug.Log("별 개수: " + itemCount);
            }

            // 군중, 박스, 물건, 가게
            if (other.tag == "People" || other.tag == "Box" || other.tag == "Thing" || other.tag == "Store" || other.tag == "Bicycle")
            {
                conflictWithCrowd = true;
                Debug.Log("Conflict with People");
            }


            if (other.name == "detect_around_pigeon")
            {
                detectAround = true;
            }

        /*if (other == pigeon.myCollider) {

        }*/
            

    }

        
    
}


   
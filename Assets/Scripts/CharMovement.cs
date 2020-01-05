using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharMovement : MonoBehaviour
{
    Joystick joyStick;
    Rigidbody rb;

    bool isJump; // 캐릭터가 점프 여부
    bool isLanding; // 캐릭터 착지 여부 
    bool isRight; // 캐릭터 오른쪽 직진 여부
    bool isLeft; // 캐릭터 왼쪽 직진 여부

    float speed = 5f;

    // Start is called before the first frame update
    void Start()
    {
        joyStick = Joystick.FindObjectOfType<Joystick>();
        rb = gameObject.GetComponent<Rigidbody>();
  
        isLanding = true;
    }

    // Update is called once per frame
    void Update()
    {

        // 점프
        if (joyStick.Vertical > 0.5f && isLanding == true)
        {
            isJump = true;
            isLanding = false;
        }

        // 오른쪽 직진
        if (joyStick.Horizontal > 0.5f && isLanding == true)
        {
            WalkToRight();
        }

        // 왼쪽 직진
        if (joyStick.Horizontal < -0.5f && isLanding == true)
        {
            WalkToLeft();
        }

    }

    private void FixedUpdate()
    {
        if(isJump == true)
            Jump();
        if(isRight == true)
            WalkToRight();
        if (isLeft == true)
            WalkToLeft();
    }

    // 점프
    void Jump ()
    {

        rb.AddForce(0f, 200f, 0f);
        isJump = false;

    }

    // 오른쪽 움직임
    void WalkToRight()
    {
        
        rb.MovePosition(rb.position + Vector3.right * speed * Time.deltaTime);
        
    }
    
    // 왼쪽 움직임
    void WalkToLeft()
    {
        rb.MovePosition(rb.position + Vector3.left * speed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.CompareTag("Ground"))
        {
            isLanding = true;
        }
    }
}

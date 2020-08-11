using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class Practice : MonoBehaviour
{
   
    //public bool Dead = false;
    Animator anim;
 
    //속도 CharacterManager에서 매개변수 보내기
    // Start is called before the first frame update
    void Awake()
    {
        anim = GetComponent<Animator>();
    }
    // Update is called once per frame

    /*
     업데이트는 항상 상태 확인임. 일단 불러오고 불러올 조건이 맞으면 상태 불러옴
     */
    void Update()
    {
        Move();
        Turn();
        Jump();

       
    }

    void Move() 
    {//나중에 달리기는 bool값으로 인식해서 구현되도록ㅎ기
        if (Input.GetKeyDown(KeyCode.A))
        {
            anim.SetFloat("Speed", 2);
        }
        else if (Input.GetKeyDown(KeyCode.B)) 
        {
            anim.SetFloat("Speed", Input.GetAxis("Vertical"));

        }
    }
  
    void Turn()
    {
        anim.SetFloat("Direction", Input.GetAxis("Horizontal"));
    }
    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            anim.SetTrigger("Jump");
        }
    }
    /* 
     * string Key = "";
     *  public string moveAxisName = "Vertical";
    public string rotateAxisName = "Horizontal";
    public string JumpName = "Jump";
     * if (Input.GetAxis("Vertical") != 0)
     {
         Key = moveAxisName;
     }
     else if (Input.GetAxis("Horizontal") != 0)
     {
         Key = rotateAxisName;
     }
     else if (Input.GetKeyDown(KeyCode.Space))
     {
         Key = JumpName;
     }*/
    /*switch (Key)
    {
        case "Vertical":
            anim.SetFloat("Speed", Input.GetAxis("Vertical"));
            break;
        case "Horizontal":
            anim.SetFloat("Direction", Input.GetAxis("Horizontal"));
            break;
        case "JumpName":
            anim.SetTrigger("Jump");
            break;
    }*/
}


/* private void Move() 
 {
     Speed = Input.GetAxis("Vertical");//키보드상하

 }
 private void Jump()
 {
     anim.SetTrigger("Jump");
 }
 private void Turn()
 {
     Direction = Input.GetAxis("Horizontal");//키보드좌우
     anim.SetFloat("Direction", Input.GetAxis("Horizontal"));
 }
}



/*
캐릭터 매니저에서
속도값을 보냄
종종 회전값도 보냄.
그리고 종종 trigger/bool값도 보냄
총3가지로 분류하기
그리고 받고나서 각각마다 어떤 상태인지 세팅하고,
그 값을 넣어서 Setfloat, Setinteger, Settrigger
최종으로 넣기

*/

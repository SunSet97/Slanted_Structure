using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Player_Anim : MonoBehaviour
{
<<<<<<< HEAD
    
=======

>>>>>>> programming/psa
    public float Speed;
    public float Direction;
    public State state;
    public bool Jump=false;
    public bool Dead=false;
    Animator anim;

    public enum State
    {
        Idle,
        Move,
        Seat,
        Jump,
        Dead

    }
<<<<<<< HEAD
   
=======

>>>>>>> programming/psa
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
        if (state !=State.Dead)
        {
<<<<<<< HEAD
            
            Move_Anim(GameObject.FindWithTag("NPC").GetComponent<CharacterManager>().Speed, Direction);
            Set_Anim();

                
=======

            Move_Anim(GameObject.FindWithTag("NPC").GetComponent<CharacterManager>().Speed, Direction);
            Set_Anim();


>>>>>>> programming/psa
        }

    }
    public void Move_Anim(float speed, float dir)//움직이는 속도 float값, 회전
    {
        if (speed>0)
        {
            anim.SetFloat("Speed", speed);
            if (dir != 0)
            {
                anim.SetFloat("Direct", dir);
            }
        }

    }

    public void Set_Anim()//상태 자체 trgger bool값
    {
        if (Dead != true)
        {
            if (state == State.Idle)
            {
            }
<<<<<<< HEAD
            
            if (Jump == true)
            {
                anim.SetTrigger("Jump"); //점프  
=======

            if (Jump == true)
            {
                anim.SetTrigger("Jump"); //점프
>>>>>>> programming/psa
                Jump = false;
            }
          /*  if (state == State.Seat)
            {
<<<<<<< HEAD
                anim.Play("Seat"); //앉기 
=======
                anim.Play("Seat"); //앉기
>>>>>>> programming/psa
                //anim.Play("Seat_Idle");//앉기Idle 유지
            }
          */


        }
        else
            anim.SetTrigger("Dead");
<<<<<<< HEAD
        
=======

>>>>>>> programming/psa
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

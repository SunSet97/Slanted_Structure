using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Animation_Controller : MonoBehaviour
{
    public Animator anim;//할당 애니메이터
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();//애니메이터 접근
    }

    // Update is called once per frame
    void Update()
    {
        //Move_Setting();//이동
        Action_Setting();//액션
    }

   /* void Move_Setting() //이동 관련
    {   
        //좌우 이동
        if (DataController.instance_DataController.currentMap.playMethod== "OneDirection")
        {
            anim.SetFloat("DirX", DataController.instance_DataController.inputDirection.x);//X방향
            anim.SetFloat("Speed", DataController.instance_DataController.inputDegree);//Speed
        }
        //상하좌우 이동
        else if (DataController.instance_DataController.currentMap.playMethod == " AllDirection")
        {
            anim.SetFloat("DirX", DataController.instance_DataController.inputDirection.x);//X방향
            anim.SetFloat("DirY", DataController.instance_DataController.inputDirection.y);//Y방향
            anim.SetFloat("Speed", DataController.instance_DataController.inputDegree);//속도
            
            //2D사이드뷰 아니어도 UI버튼 통해서 구현될 수 있어서 넣음.
            if (DataController.instance_DataController.inputJump == true)
            {
                anim.SetTrigger("Jump");
                DataController.instance_DataController.inputJump = false;
            }
            if (DataController.instance_DataController.inputDash == true)
            {
                anim.SetTrigger("Dash");
                DataController.instance_DataController.inputDash = false;
            }
        }
    }*/

    void Action_Setting() //액션 관련
    {
        if (DataController.instance_DataController.inputJump == true)//점프
        {
            anim.SetTrigger("Jump");
            DataController.instance_DataController.inputJump = false;//트리거 이후 false로 바꾸기
        }
        if (DataController.instance_DataController.inputDash == true)//대시
        {
            anim.SetTrigger("Dash");
            DataController.instance_DataController.inputDash = false;//트리거 이후 false로 바꾸기
        }
        
        
        //Seat
        
        /*  Seat이라는 값이 datacontroller에 추가하고 넣을 예정.
         *  아직 입력인식이 따로 없음. 
         *  인터렉션 으로 의자 주변에 있을 때, 의자 자체에 트리거가 활성화 되고, 
         *  터치하면 bool값이 변경되는 것으로 세팅할 필요가 있음!
         * 
        if (anim.GetBool("Seat") == false)//or DataController.instance_DataController.Seat==true
            {
                anim.SetBool("Seat", true);//앉기
            }
        else
            anim.SetBool("Seat", false);//일어나
        */

        //Dead

        /*
         * 얘도 마찬가지로 죽음 상태를 DataController에 bool값 추가하여
         * 플레이어의 상태값을 인지하고 Dead가 true이면 사망 상태의 애니메이션이 활성화되고
         * 게임 오버 or 리스폰 할 수 있도록 해야할 듯.
         * 
        if (anim.GetBool("Dead")== true)// or  DataController.instance_DataController.Dead==true
        {
            anim.SetBool("Dead", true);//사망
        }
         */
    }


    public void Onclicked(string Action)//이동을 제외한 애니메이션을  임시 버튼을 통해 활성화 시키는 것.
    {
        //string Action_state;
        if (Action == "Jump") //점프
        {
            anim.SetTrigger("Jump");
            Debug.Log("Jump");
        }
        else if (Action == "Dash") //대시
        {
            anim.SetTrigger("Dash");
            Debug.Log("Dash");
        }
        else if (Action == "Seat")//앉기
        {
            Debug.Log("Seat");
            if (anim.GetBool("Seat") == false)//서있을 때
            {
                anim.SetBool("Seat", true);//앉기
            }
            else
                anim.SetBool("Seat", false);//일어나

        }
        else if (Action == "Dead")
        {
            Debug.Log("Dead");
            anim.SetBool("Dead",true);//사망
        }
        else if (Action == "Restart")
        {
            SceneManager.LoadScene("Set_Animation");//재 시작
        }
    }
}

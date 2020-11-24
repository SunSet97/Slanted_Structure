using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Animation_Controller : MonoBehaviour
{
    public enum Emotion {Idle,Laugh,Sad,Cry,Angry,Surprise,Panic,Suspicion,Fear,Curious };//감정상태
    public Emotion emotion_present;
    public SkinnedMeshRenderer Char_talking;//현재 말하는 캐릭터
    public Animator anim; //할당 애니메이터
    public Texture[] faceExpression;//표정 메터리얼

    void Start()
    {
        anim = GetComponent<Animator>(); //애니메이터 접근
        faceExpression = Resources.LoadAll<Texture>("Face");//배열에 넣기

        anim.SetInteger("Emotion", 0);//기본 애니메이션
        //Char_talking.materials[0].SetTexture("_MainTex", faceExpression[11]);
        //Char_talking.materials[1].SetTexture("_MainTex",faceExpression[0]);//캐릭터의 첫번째에는 옷, 두번째는 표정을 넣어야함

    }
        
    void Update()
    {
        Emotion_Setting();
        Action_Setting();//액션
        if (SceneManager.GetActiveScene().name == "Cinematic")
        {
            Emotion_Setting();
        }
        if (SceneManager.GetActiveScene().name == "Ingame") 
        {
            Char_talking.materials[1].SetTexture("_MainTex", faceExpression[0]);
        }
    
    }
    void Emotion_Setting() //현재 Emotion상태값 넣기
    {
        anim.SetInteger("Emotion",(int)emotion_present);//애니메이션실행
        Char_talking.materials[1].SetTexture("_MainTex", faceExpression[(int)emotion_present]);//현재 감정으로 메터리얼 변경
    }

    void Action_Setting() //액션 관련
    {
        if (DataController.instance_DataController.inputJump == true)//점프
        {
            DataController.instance_DataController.currentChar.moveVerDir.y+= DataController.instance_DataController.currentChar.jumpForce;
            anim.SetTrigger("Jump");
            DataController.instance_DataController.inputJump = false;//트리거 이후 false로 바꾸기
        }

        if (DataController.instance_DataController.inputDash == true)//대시
        {
            anim.SetTrigger("Dash");
            DataController.instance_DataController.inputDash = false;//트리거 이후 false로 바꾸기
        }

        //Seat
        //  Seat이라는 값이 datacontroller에 추가하고 넣을 예정.
          
         /*  인터렉션 으로 의자 주변에 있을 때, 의자 자체에 트리거가 활성화 되고, /희원이랑 이야기해야될 부분
         *  터치하면 bool값이 변경되는 것으로 세팅할 필요가 있음!*/
        if (DataController.instance_DataController.inputSeat==true)
            {
                anim.SetBool("Seat", true);//앉기
            }
        else
            anim.SetBool("Seat", false);//일어나
        

        //Dead

        /*얘도 마찬가지로 죽음 상태를 DataController에 bool값 추가하여
        플레이어의 상태값을 인지하고 Dead가 true이면 사망 상태의 애니메이션이 활성화되고
        게임 오버 or 리스폰 할 수 있도록 해야할 듯.*/

        if (anim.GetBool("Dead")== true)// or  DataController.instance_DataController.Dead==true
        {
            anim.SetBool("Dead", true);//사망
        }
         
    }


    

    
}

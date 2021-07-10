﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterManager : MonoBehaviour
{
    #region 기본 설정
    [Header("#Default inspector setting(auto)")]
    public Joystick joyStick; // 조이스틱
    public CharacterController ctrl; // 캐릭터컨트롤러

    public Animator anim; // 애니메이션
    private SkinnedMeshRenderer skinnedMesh; // 캐릭터 머테리얼
    private Texture[] faceExpression;//표정 메터리얼

    private Camera cam; // 카메라

    void Start()
    {
        ctrl = this.GetComponent<CharacterController>();
        anim = this.GetComponent<Animator>();
        // 표정 머테리얼 초기화
        faceExpression = Resources.LoadAll<Texture>("Face");
        skinnedMesh = this.GetComponentInChildren<SkinnedMeshRenderer>();
        skinnedMesh.materials[1].SetTexture("_MainTex", faceExpression[(int)emotion]);
    }

    void Update()
    {
        // 조이스틱 설정
        if (!joyStick && DataController.instance_DataController.joyStick) joyStick = DataController.instance_DataController.joyStick;

        // 카메라 설정
        if (!cam && DataController.instance_DataController.cam) cam = DataController.instance_DataController.cam;

        // 에니메이션 설정
        AnimationSetting();
    }
    #endregion

    #region 캐릭터 컨트롤
    [Header("#Character pick up controll")]
    public bool isControlled;   // 움직일 수 있는 여부
    public bool isSelected;     // 선택여부
    // 캐릭터를 스크립트로 직접 이동할 수 있게 함 (캐릭터를 손으로 집는다고 생각)
    public void PickUpCharacter()
    {
        isControlled = false;
        anim.applyRootMotion = false;
    }

    // 캐릭터를 스크립트로 직접 이동할 수 있게 함 (캐릭터를 손으로 집는다고 생각)
    public void PutDownCharacter()
    {
        isControlled = isSelected;
        anim.applyRootMotion = true;
    }

    // 일정 시간 후 캐릭터를 조이스틱으로 움직이게 함
    public void UseJoystickCharacter()
    {
        Invoke("PutDownCharacter", 0.2f);
    }
    #endregion

    #region 캐릭터 애니메이션 설정
    // 감정상태
    public enum Emotion { Idle, Laugh, Sad, Cry, Angry, Surprise, Panic, Suspicion, Fear, Curious };
    public Emotion emotion = Emotion.Idle;      // 캐릭터 감정

    // 에니메이션 설정
    void AnimationSetting()
    {
        EmotionAnimationSetting();
        ActionAnimationSetting();
    }

    // 현재 Emotion상태값 넣기
    void EmotionAnimationSetting()
    {
        if (SceneManager.GetActiveScene().name == "Cinematic")
        {
            anim.SetInteger("Emotion", (int)emotion); // 애니메이션실행
        }
        skinnedMesh.materials[1].SetTexture("_MainTex", faceExpression[(int)emotion]); // 현재 감정으로 메터리얼 변경


    }

    // 액션 관련
    void ActionAnimationSetting()
    {
        if (SceneManager.GetActiveScene().name == "Ingame")
        {
            // 대시
            if (DataController.instance_DataController.inputDash == true)
            {
                anim.SetTrigger("Dash");
                DataController.instance_DataController.inputDash = false;
            }

            // 앉기
            /*  인터렉션 으로 의자 주변에 있을 때, 의자 자체에 트리거가 활성화 되고, /희원이랑 이야기해야될 부분
            *  터치하면 bool값이 변경되는 것으로 세팅할 필요가 있음!*/
            if (DataController.instance_DataController.inputSeat == true)
                anim.SetBool("Seat", true);
            else
                anim.SetBool("Seat", false);

            // 사망
            /*죽음 상태를 DataController에 bool값 추가하여
            플레이어의 상태값을 인지하고 Dead가 true이면 사망 상태의 애니메이션이 활성화되고
            게임 오버 or 리스폰 할 수 있도록 해야할 듯.*/
            // anim.SetBool("Dead", true);
        }
    }
    #endregion

    #region 캐릭터 이동 설정
    [Header("#Character move setting")]
    public Vector3 moveHorDir = Vector3.zero, moveVerDir = Vector3.zero;    // 수평, 수직 이동 방향 벡터
    private Vector2 joystickDir;
    private Vector2 characterDir;
    public float joyRot;
    public Quaternion camRotation; // 메인 카메라 기준으로 joystick input 변경(라인트레이서 제외)

    public bool isJump;                     // 캐릭터의 점프 여부
    public float jumpForce = 5f;            // 점프력
    public float gravityScale = 0.6f;       // 중력 배수
    public float airResistance = 1.2f;      // 공기 저항

    private void FixedUpdate()
    {
        // 조이스틱 설정이 끝난 이후 이동 가능, 캐릭터를 조종할 수 있을 때
        if (joyStick && cam && ctrl.enabled && isControlled)
        {
            // 메인 카메라 기준으로 캐릭터가 바라보는 방향 계산
            camRotation = Quaternion.Euler(0, -cam.transform.rotation.eulerAngles.y, 0);
            Vector3 transformedDir = camRotation * transform.forward;
            characterDir = new Vector2(transformedDir.x, transformedDir.z);
            // 조이스틱이 가리키는 방향
            joystickDir = new Vector2(DataController.instance_DataController.inputDirection.x, DataController.instance_DataController.inputDirection.y);

            joyRot = Vector2.SignedAngle(joystickDir, characterDir);
            if (Mathf.Abs(joyRot) > 0) { this.transform.Rotate(Vector3.up, joyRot); } // 임시 회전
            /* 180도 회전
            if (Mathf.Abs(joyRot) > 170 && !anim.GetBool("180Turn"))
                anim.SetBool("180Turn", true);
            else
                anim.SetBool("180Turn", false);
            */

            anim.SetFloat("Direction", joyRot); //X방향
            anim.SetFloat("Speed", DataController.instance_DataController.inputDegree); //Speed     ####################################################애니메이션할 때 참고..

            if (DataController.instance_DataController.currentMap.SideView == true)
                anim.SetBool("2DSide", true);
            else
                anim.SetBool("2DSide", false);

            //점프는 바닥에 닿아 있을 때 위로 스와이프 했을 경우에 가능(쿼터뷰일때 불가능)
            if (isSelected && DataController.instance_DataController.inputJump && ctrl.isGrounded)
                anim.SetBool("Jump", true);  //점프 가능 상태로 변경

            //캐릭터 선택중일때 점프 가능
            if (isSelected && DataController.instance_DataController.inputJump && anim.GetBool("Jump"))
            {
                moveVerDir.y += jumpForce; //점프력 만큼 힘을 가함

                anim.SetBool("Jump", false); //점프 불가능 상태로 변경하여 연속적인 점프 제한
            }

            //땅에서 떨어져 있을 경우 기본적으로 중력이 적용되고 중력은 가속도이므로 +=를 써서 계속해서 더해줌
            if (!ctrl.isGrounded)
                moveVerDir.y += Physics.gravity.y * gravityScale * Time.deltaTime;

            //if (DataController.instance_DataController.isMapChanged == false)
            ctrl.Move((moveHorDir + moveVerDir) * Time.deltaTime); //캐릭터를 최종 이동 시킴
        }
    }
    #endregion
}
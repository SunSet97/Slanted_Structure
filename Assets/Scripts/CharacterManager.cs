using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    #region 기본 설정
    [Header("#Default inspector setting(auto)")]
    public Joystick joyStick; // 조이스틱
    public CharacterController ctrl; // 캐릭터컨트롤러
    private Animator anim; // 애니메이션
    public Animation_Controller Anim_Controller; // 애니메이션 컨트롤러
    private Camera cam; // 카메라

    void Start()
    {
        ctrl = this.GetComponent<CharacterController>();
        anim = this.GetComponent<Animator>();
        Anim_Controller = this.GetComponent<Animation_Controller>();
    }

    void Update()
    {
        // 조이스틱 설정
        if (!joyStick && DataController.instance_DataController.joyStick) joyStick = DataController.instance_DataController.joyStick;

        // 카메라 설정
        if (!cam && DataController.instance_DataController.cam) cam = DataController.instance_DataController.cam;

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

    #region 캐릭터 이동 설정
    [Header("#Character move setting")]
    public Vector3 moveHorDir = Vector3.zero, moveVerDir = Vector3.zero;    // 수평, 수직 이동 방향 벡터
    private Vector2 joystickDir;
    private Vector2 characterDir;
    private float joyRot;
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
            camRotation = Quaternion.Euler(0, cam.transform.rotation.eulerAngles.y, 0);
            Vector3 transformedDir = camRotation * transform.forward;
            characterDir = new Vector2(transform.forward.x, transform.forward.z);
            // 조이스틱이 가리키는 방향
            joystickDir = new Vector2(DataController.instance_DataController.inputDirection.x, DataController.instance_DataController.inputDirection.y);

            joyRot = Vector2.SignedAngle(joystickDir, characterDir);
            if (Mathf.Abs(joyRot) > 170 && !anim.GetBool("180Turn")) anim.SetBool("180Turn", true);
            else anim.SetBool("180Turn", false);
            anim.SetFloat("Direction", joyRot); //X방향
            anim.SetFloat("Speed", DataController.instance_DataController.inputDegree); //Speed
            if (DataController.instance_DataController.currentMap.SideView == true)
                anim.SetBool("2DSide", true);
            else
                anim.SetBool("2Dside", false);
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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public Joystick joyStick; // 조이스틱
    public CharacterController ctrl; // 캐릭터컨트롤러
    private Animator anim; // 애니메이션
    public Animation_Controller Anim_Controller;//애니메이션 컨트롤러

    private CanvasControl canvasCtrl;

    public Vector3 moveHorDir = Vector3.zero, moveVerDir = Vector3.zero;    // 수평, 수직 이동 방향 벡터

    private IEnumerator dieAction;
    private bool swipeGrass; // 풀 숲 헤쳐나갈 때 사용



    private Vector2 joystickDir;
    private Vector2 characterDir;
    private float joyRot;

    [Header("캐릭터 정보")]
    public bool isControlled;   // 움직일 수 있는 여부
    public bool isSelected;     // 선택여부
    public bool isExisted;      // 맵 존재여부(데이터 로드시 사용)

    public bool isAbility;      // 능력 사용 여부

    [Header("시점")]
    public Camera cam;

    [Header("디버깅용")]
    public float Speed = 0;                 // 이동 속도
    public float maxMoveSpeed = 2f;         // 최대 이동 속도
    public float moveAcceleration = 14f;    // 이동 가속도
    public float airResistance = 1.2f;      // 공기 저항
    public float frictionalForce = 4f;      // 지상 마찰력
    public bool isJump;                     // 캐릭터의 점프 여부
    public float jumpForce = 5f;            // 점프력
    public float gravityScale = 1.1f;       // 중력 배수
    public bool isDie = false;              // 죽음 여부
    public bool splitTest = false;                  // 코드 분리 임시 변수

    public Quaternion camRotation; // 메인 카메라 기준으로 joystick input 변경(라인트레이서 제외)
    public float moveSpeed;        // 현재의 수평 이동 속도 계산
    public Vector3 unitVector;     //이동 방향 기준 단위 벡터
    public float normalizing;      //조이스틱 입력 강도 정규화

    void Start()
    {
        ctrl = this.GetComponent<CharacterController>();
        anim = this.GetComponent<Animator>();

        canvasCtrl = CanvasControl.instance_CanvasControl;
        swipeGrass = false;

        // 세이브데이터를 불러왔을 경우 저장된 위치로 캐릭터 위치를 초기화
        if (DataController.instance_DataController != null)
        {
            // 디버깅용
            DataController.instance_DataController.charData.pencilCnt = 4;
            DataController.instance_DataController.charData.selfEstm = 500;
            DataController.instance_DataController.charData.intimacy_spRau = 200;
            DataController.instance_DataController.charData.intimacy_spOun = 150;
            DataController.instance_DataController.charData.intimacy_ounRau = 150;
            DataController.instance_DataController.charData.story = 1;
            DataController.instance_DataController.charData.storyBranch = 2;
            DataController.instance_DataController.charData.storyBranch_scnd = 3;
            DataController.instance_DataController.charData.dialogue_index = 4;
        }
    }

    void Update()
    {
        // 조이스틱 설정
        if (!joyStick && DataController.instance_DataController.joyStick) joyStick = DataController.instance_DataController.joyStick;



        // 카메라 설정
        if (!cam && DataController.instance_DataController.cam) cam = DataController.instance_DataController.cam;

        //// 라우 튜토리얼 풀 숲 지나갈 때
        //if (swipeGrass && ctrl.enabled == false && canvasCtrl.finishFadeIn) SwipeGrass();
    }

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
        Invoke("PutDownCharacter", 0.5f);
    }

    #region ToDo

    private void FixedUpdate()
    {
        if (splitTest)
            return;

        // 조이스틱 설정이 끝난 이후 이동 가능, 캐릭터를 조종할 수 있을 때
        if (joyStick && cam && ctrl.enabled && isControlled) CharacterMovement();
    }

    // 캐릭터 움직임 코드
    private void CharacterMovement()
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

        //점프는 바닥에 닿아 있을 때 위로 스와이프 했을 경우에 가능(쿼터뷰일때 불가능)
        if (isSelected && DataController.instance_DataController.inputJump && ctrl.isGrounded)
            anim.SetBool("Jump", true);  //점프 가능 상태로 변경

        //캐릭터 선택중일때 점프 가능
        if ((isSelected && !isDie) && DataController.instance_DataController.inputJump && anim.GetBool("Jump"))
        {
            moveVerDir.y += jumpForce; //점프력 만큼 힘을 가함

            anim.SetBool("Jump", false); //점프 불가능 상태로 변경하여 연속적인 점프 제한
        }

        //땅에서 떨어져 있을 경우 기본적으로 중력이 적용되고 중력은 가속도이므로 +=를 써서 계속해서 더해줌
        if (!ctrl.isGrounded || isDie)
            moveVerDir.y += Physics.gravity.y * gravityScale * Time.deltaTime;

        //if (DataController.instance_DataController.isMapChanged == false)
        ctrl.Move((moveHorDir + moveVerDir) * Time.deltaTime); //캐릭터를 최종 이동 시킴
    }
    #endregion

    //// 플레이어가 죽으면 불리는 함수
    //void CharDie()
    //{
    //    StartCoroutine(dieAction);
    //}

    #region SpeatAbility
    public void Change_Position(bool button_on)
    {
        if (!button_on)
        {
            ctrl.enabled = false;
        }
        if (button_on)
        {
            ctrl.enabled = true;
        }

    }

    public void CanWallPass(int layerNum)
    {
        this.gameObject.layer = layerNum;
    }
    public void CannotWallPass(int layerNum)
    {
        this.gameObject.layer = 0;
    }
    #endregion
}

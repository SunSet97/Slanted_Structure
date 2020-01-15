using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharMovement : MonoBehaviour
{
    Joystick joyStick; //조이스틱
    CharacterController ctrl; //캐릭터컨트롤러
    public Vector3 moveHorDir = Vector3.zero; //수평 이동 방향 벡터
    public Vector3 moveVerDir = Vector3.zero; //수직 이동 방향 벡터

    [Header("시점")]
    public Camera cam;
    public bool isPlatformView = true; //2D 플랫포머
    public bool isOutsideQuarterView = false; //쿼터뷰 건물 외부
    public bool isInsideQuarterView = false; //쿼터뷰 건물 내부

    [Header("디버깅용")]
    public float maxMoveSpeed = 4f; //최대 이동속도
    public float moveAcceleration = 8f; //이동가속도
    public float airResistance = 3f; //공기저항
    public float frictionalForce = 5f; //지상 마찰력
    public bool isJump; // 캐릭터가 점프 여부
    public float jumpForce = 6f; //점프력
    public float gravityScale = 1f; //중력 배수

    void Start()
    {
        joyStick = Joystick.FindObjectOfType<Joystick>();
        ctrl = GetComponent<CharacterController>();
    }

    void Update()
    {
        //점프는 바닥에 닿아 있을 때 위로 스와이프 했을 경우에 가능(건물 내부일때만 불가능)
        if (joyStick.Vertical > 0.5f && ctrl.isGrounded && !isInsideQuarterView)
        {
            isJump = true;  //점프 가능 상태로 변경
        }      
    }

    private void FixedUpdate()
    {
        //현재의 수평 이동 속도 계산
        float moveSpeed = Mathf.Sqrt(moveHorDir.x * moveHorDir.x + moveHorDir.z * moveHorDir.z);

        Vector3 unitVector = Vector3.zero; //이동 방향 기준 단위 벡터
        float input = 0; //조이스틱 입력 강도
        Quaternion camRotation = Quaternion.Euler(0, cam.transform.rotation.eulerAngles.y, 0); //메인 카메라 기준으로 joystick input 변경
        //시점에 따라 이동하는 기준 방향 설정
        if (isPlatformView) //2D 플랫폼
        {
            //조이스틱 입력이 0일 경우는 입력이 없거나 조이스틱을 뗐을 때
            if (joyStick.Horizontal == 0)
                unitVector = moveHorDir.normalized; //현재 움직이는 방향이 마지막으로 입력된 정방향
            else
                unitVector = camRotation * (Vector3.right * joyStick.Horizontal).normalized; //현재 입력되는 방향이 정방향(x축)
            input = Mathf.Abs(joyStick.Horizontal); //수평 성분이 입력의 세기
        }
        else if (isOutsideQuarterView) //쿼터뷰 외부
        {
            //조이스틱 입력이 0일 경우는 입력이 없거나 조이스틱을 뗐을 
            if (joyStick.Horizontal == 0 && joyStick.Vertical == 0)
                unitVector = moveHorDir.normalized; //현재 움직이는 방향이 마지막으로 입력된 정방향
            else
                unitVector = (Waypoint.FindObjectOfType<Waypoint>().moveTo * joyStick.Horizontal).normalized; //현재 입력되는 방향이 정방향(waypoint연결선)
            input = Mathf.Abs(joyStick.Horizontal); //수평 성분이 입력의 세기(횡스크롤이므로)
        }
        else if (isInsideQuarterView) //쿼터뷰 내부
        {
            //조이스틱 입력이 0일 경우는 입력이 없거나 조이스틱을 뗐을 때
            if (joyStick.Horizontal == 0 && joyStick.Vertical == 0)
                unitVector = moveHorDir.normalized; //현재 움직이는 방향이 마지막으로 입력된 정방향
            else
                unitVector = camRotation * new Vector3(joyStick.Horizontal, 0, joyStick.Vertical).normalized; //현재 입력되는 방향이 정방향(xz평면)
            input = new Vector3(joyStick.Horizontal, 0, joyStick.Vertical).sqrMagnitude; //수평 수직의 벡터 크기가 입력의 세기
        }

        //isGrounded에 따라 저항력 결정
        float resistanceForce = 0; //저항력
        if (ctrl.isGrounded)
            resistanceForce = frictionalForce; //캐릭터가 땅에 붙어있을때 마찰력 적용
        else
            resistanceForce = airResistance; //캐릭터가 땅에 벗어나있을때 공기저항 적용


        //좌우 이동시 최대 이동속도를 제한하여 일정 속도를 넘지 않도록함
        if (ctrl.isGrounded && moveSpeed <= maxMoveSpeed)
            moveHorDir += unitVector * input * moveAcceleration * Time.deltaTime; //조이스틱의 방향과 세기에 따라 해당 이동 방향으로 가속도 작용

        //이동중 일때 마찰력 적용
        if (moveSpeed >= 0.05f)
        {
            if ((moveHorDir.normalized - unitVector).sqrMagnitude < 0.8f)
                moveHorDir -= unitVector * resistanceForce * Time.deltaTime; //이동 방향과 기준 방향 차의 크기가 1보다 작으면 같은 방향(정방향)으로 이동 중, 따라서 저항력의 방향은 기준 방향의 역방향으로 작용
            else
                moveHorDir += unitVector * resistanceForce * Time.deltaTime; //이동 방향과 기준 방향 차의 크기가 1보다 크면 다른 방향(역방향)으로 이동 중, 따라서 저항력의 방향은 기준 방향의 정방향으로 작용
        }

        //점프
        if (isJump)
        {
            moveVerDir.y = jumpForce; //점프력 만큼 힘을 가함
            isJump = false; //점프 불가능 상태로 변경하여 연속적인 점프 제한
        }

        //땅에서 떨어져 있을 경우 기본적으로 중력이 적용되고 중력은 가속도이므로 +=를 써서 계속해서 더해줌
        if (!ctrl.isGrounded)
            moveVerDir.y += Physics.gravity.y * gravityScale * Time.deltaTime;

        ctrl.Move((moveHorDir + moveVerDir) * Time.deltaTime); //캐릭터를 이동 시킴
    }
}

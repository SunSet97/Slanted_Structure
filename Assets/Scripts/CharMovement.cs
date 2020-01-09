using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharMovement : MonoBehaviour
{
    Joystick joyStick; //조이스틱
    CharacterController ctrl; //캐릭터컨트롤러
    Vector3 moveDir = Vector3.zero; //이동 방향 벡터

    [Header("디버깅용")]
    public float maxMoveSpeed = 5f; //최대 이동속도
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
        //점프는 바닥에 닿아 있을 때 위로 스와이프 했을 경우에 가능
        if (joyStick.Vertical > 0.5f && ctrl.isGrounded)
        {
            isJump = true;  //점프 가능 상태로 변경
        }

    }

    private void FixedUpdate()
    {
        //좌우 이동시 최대 이동속도를 제한하여 일정 속도를 넘지 않도록함
        if (ctrl.isGrounded && Mathf.Abs(moveDir.x) <= maxMoveSpeed)
            moveDir.x += joyStick.Horizontal * moveAcceleration * Time.deltaTime; //조이스틱의 방향에 따라 해당 이동 방향으로 가속도 작용

        //캐릭터가 땅에 붙어있으면서 움직이는 상태일 때 마찰력 적용
        if (ctrl.isGrounded && moveDir.x != 0)
            moveDir.x -= (moveDir.x / Mathf.Abs(moveDir.x)) * frictionalForce * Time.deltaTime; //마찰력의 방향은 이동 방향의 반대 방향으로 작용
        //캐릭터가 땅에 벗어나있으면 좌우 이동할 때 공기저항 적용
        if (!ctrl.isGrounded && moveDir.x != 0)
            moveDir.x -= (moveDir.x / Mathf.Abs(moveDir.x)) * airResistance * Time.deltaTime; //공기저항의 방향은 이동 방향의 반대 방향으로 작용

        //점프
        if (isJump)
        {
            moveDir.y = jumpForce; //점프력 만큼 힘을 가함
            isJump = false; //점프 불가능 상태로 변경하여 연속적인 점프 제한
        }

        //땅에서 떨어져 있을 경우 기본적으로 중력이 적용되고 중력은 가속도이므로 +=를 써서 계속해서 더해줌
        if (!ctrl.isGrounded)
            moveDir.y += Physics.gravity.y * gravityScale * Time.deltaTime;

        ctrl.Move(moveDir * Time.deltaTime); //캐릭터를 이동 시킴
    }
}

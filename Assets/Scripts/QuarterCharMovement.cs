using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuarterCharMovement : MonoBehaviour
{
    Joystick joyStick;
    CharacterController ctrl;

    [Header("디버깅용")]
    [Range(1, 5)]
    public float moveSpeed;

    private Camera cam;
    private Quaternion camRotation;
    private Vector3 movement;

    void Start()
    {
        joyStick = Joystick.FindObjectOfType<Joystick>();
        ctrl = GetComponent<CharacterController>();

        // 카메라가 y축을 기준으로 회전한 각도(기본 euler 45도)
        // 이부분을 Update() 로 넘기면 카메라 회전에 대응 가능
        cam = FindObjectOfType<Camera>();
        camRotation = Quaternion.Euler(0, cam.transform.rotation.eulerAngles.y, 0);
        movement = Vector3.zero;
    }

    void Update()
    {

    }

    private void FixedUpdate()
    {
        Vector3 joyStickInput = new Vector3(joyStick.Horizontal, 0, joyStick.Vertical);
        // JoyStick 입력을 카메라 각도만큼 회전
        movement = camRotation * joyStickInput * moveSpeed;

        ctrl.Move(movement * Time.fixedDeltaTime);
    }
}

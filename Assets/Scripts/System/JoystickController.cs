using System;
using Data;
using UnityEngine;

public class JoystickController : MonoBehaviour
{
    [Header("조이스틱")] public Joystick joyStick;
    public Vector2 inputDirection = Vector2.zero; // 조이스틱 입력 방향
    public float inputDegree = 0f; // 조이스틱 입력 크기
    public bool inputJump = false; // 조이스틱 입력 점프 판정
    public bool inputDash = false; // 조이스틱 입력 대쉬 판정
    public bool inputSeat = false;
    private bool isAlreadySave;
    private bool wasJoystickUse;

    
    private static JoystickController _instance;
    
    public static JoystickController instance => _instance;

    private void Awake()
    {
        _instance = this;
    }

    public void InitSaveLoad()
    {
        isAlreadySave = false;
    }
    
    /// <summary>
    /// 조이스틱 상태 초기화하는 함수
    /// </summary>
    /// <param name="isOn">JoyStick On/Off</param>
    public void InitializeJoyStick(bool isOn)
    {
        joyStick.gameObject.SetActive(isOn);
        joyStick.transform.GetChild(0).gameObject.SetActive(false);
        joyStick.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().position = default;
        joyStick.OnPointerUp();
        inputDegree = 0;
        inputDirection = Vector2.zero;
        joyStick.input = Vector2.zero;
        inputJump = false;
    }
    
    /// <summary>
    /// 조이스틱 멈추고 이전 상태에 따라 키거나 끄는 함수
    /// ex) 대화 이전에 조이스틱을 사용하지 않으면 계속 사용하지 않는다.
    /// </summary>
    /// <param name="isStop">Save여부   true - save, false - load</param>
    public void StopSaveLoadJoyStick(bool isSave)
    {
        Debug.Log((isSave ? "Save" : "Load") +  ", " + (isAlreadySave ? "저장된 상태" : "저장되지 않은 상태"));
        //처음 실행하는 경우
        if (isSave)
        {
            if(isAlreadySave) return;
            
            isAlreadySave = true;
            wasJoystickUse = joyStick.gameObject.activeSelf;
            InitializeJoyStick(false);
        }
        // Load하는 경우
        else
        {
            Debug.Log("저장된 상태 - " + wasJoystickUse);
            isAlreadySave = false;
            InitializeJoyStick(wasJoystickUse);
        }
    }
    
    public void SetJoystickArea(CustomEnum.JoystickAreaType joystickAreaType)
    {
        if (joystickAreaType == CustomEnum.JoystickAreaType.DEFAULT)
        {
            
        }else if (joystickAreaType == CustomEnum.JoystickAreaType.FULL)
        {
            
        }else if (joystickAreaType == CustomEnum.JoystickAreaType.NONE)
        {
            
        }
    }
    
    public void JoystickInputUpdate(CustomEnum.JoystickInputMethod method)
    {
        var joystickController = JoystickController.instance;
        //사이드뷰 일 때
        if (method.Equals(CustomEnum.JoystickInputMethod.OneDirection))
        {
            joystickController.inputDegree =
                Mathf.Abs(joystickController.joyStick.Horizontal); // 조정된 입력 방향으로 크기 계산
            joystickController.inputDirection.Set(joystickController.joyStick.Horizontal, 0); // 조정된 입력 방향 설정
            joystickController.inputJump =
                joystickController.joyStick.Vertical > 0.5f; // 수직 입력이 일정 수치 이상 올라가면 점프 판정
        }
        else
        {
            joystickController.inputDirection.Set(joystickController.joyStick.Horizontal,
                joystickController.joyStick.Vertical); // 조정된 입력 방향 설정
            joystickController.inputDegree =
                Vector2.Distance(Vector2.zero, joystickController.inputDirection); // 조정된 입력 방향으로 크기 계산
            joystickController.inputJump = false;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Data.CustomEnum;

public class JoystickInputManager : MonoBehaviour
{
    public static JoystickInputManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void JoystickInputUpdate(JoystickInputMethod method)
    {
        //사이드뷰 일 때
        if (method.Equals(JoystickInputMethod.OneDirection))
        {
            DataController.instance.inputDegree =
                Mathf.Abs(DataController.instance.joyStick.Horizontal); // 조정된 입력 방향으로 크기 계산
            DataController.instance.inputDirection.Set(DataController.instance.joyStick.Horizontal, 0); // 조정된 입력 방향 설정
            DataController.instance.inputJump =
                DataController.instance.joyStick.Vertical > 0.5f; // 수직 입력이 일정 수치 이상 올라가면 점프 판정
        }
        else
        {
            DataController.instance.inputDirection.Set(DataController.instance.joyStick.Horizontal,
                DataController.instance.joyStick.Vertical); // 조정된 입력 방향 설정
            DataController.instance.inputDegree =
                Vector2.Distance(Vector2.zero, DataController.instance.inputDirection); // 조정된 입력 방향으로 크기 계산
            DataController.instance.inputJump = false;
        }
    }
}

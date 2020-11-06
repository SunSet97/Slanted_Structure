using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RauTutorialManager : MonoBehaviour
{
    public MapData mapdata;
    public Swipe swipeDir;

    public enum Swipe
    {
        Left,
        Right,
        Down
    }
    
    // Update is called once per frame
    void Update()
    {

    }

    // 슬라이드 판정
    void TouchSlide()
    {
        // https://www.youtube.com/watch?v=98dQBWUyy9M 멀티 터치 참고
        if (DataController.instance_DataController.joyStick.Horizontal > 0) swipeDir = Swipe.Right;
        else if (DataController.instance_DataController.joyStick.Horizontal < 0) swipeDir = Swipe.Left;
        else if (DataController.instance_DataController.joyStick.Vertical < 0) swipeDir = Swipe.Down;
    }

    // 조이스틱 이미지 껐다 끄기
    void OnOffJoystick(bool isOn)
    {
        foreach (Image image in DataController.instance_DataController.joyStick.GetComponentsInChildren<Image>())
        {
            if (isOn && image.name != "Transparent Dynamic Joystick") image.color = Color.white;
            else image.color = Color.clear;
        }
    }

    // 이동 방식 변환 함수
    void ChangeJoystickSetting(int methodNum, int axisNum)
    {
        mapdata.method = (MapData.JoystickInputMethod)methodNum; // 0 = one dir, 1 = all dir, 2 = other
        OnOffJoystick(methodNum == 2); // other일 경우 인터렉션 부분이므로 조이스틱 안보이게 함
        DataController.instance_DataController.joyStick.AxisOptions = (AxisOptions)axisNum; // 0 = both, 1 = hor, 2 = ver
    }

    // 숲 초입길
    void ForestIntro()
    {
        // 카메라 방향 side
        // side 이동 및 점프 튜토리얼
        ChangeJoystickSetting(0, 0); // 2D side view 이동
    }

    // 수풀길
    void Grass()
    {
        // 카메라 방향 앞, 어깨 뒤 방향
        // 수풀길 헤쳐가기
        ChangeJoystickSetting(2, 1); // 이동 해제, 좌우 스와이프만 가능하도록 변경

        // 수풀 끝
        ChangeJoystickSetting(0, 0); // 2D side view 이동, 조이스틱 입력 리셋
    }

    // 물가
    void River()
    {
        // 카메라 방향 side
        ChangeJoystickSetting(0, 0); // 2D side view 이동

        // 인터렉트할 물건 찾기
        // 나무 발로 차서 넘어뜨리기
        ChangeJoystickSetting(2, 0); // 이동 해제

        // 카메라 방향 side
        ChangeJoystickSetting(0, 0); // 2D side view 이동
    }

    // 나무 숲
    void Forest()
    {
        // 카메라 방향 앞, 쿼터뷰
        // 둘러보기, 전방향 이동 튜토리얼
        ChangeJoystickSetting(1, 0); // 전방향 이동
        
        // 특정 지점에서 나무 쓰러짐
        // 스와이프로 나무 넘어가기
        ChangeJoystickSetting(2, 2); // 이동 해제, 위아래 스와이프만 가능하도록 변경

        // 시네마틱 씬 전환
    }
}

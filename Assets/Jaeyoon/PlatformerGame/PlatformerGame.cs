using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerGame : MonoBehaviour
{
    //2d화면으로 보일때의 왼쪽 하단 or 오른쪽 하단 button 생성
    public ButtonType btnType;
    Vector3 state_decide;
    
    private void Start()
    {
        state_decide = transform.position;
        
        
    }
    //joystick jump 구현되어있나?

    //판정이 되는 순간 버튼도 보이도록
    public void ActiveButton() { 
        //joystick이용해서 이동하다가 fence or wall 경계선 안에 있을 경우 판정되면
        //if(line을 지나서 범위안에 있을 경우 button 보이게) {}
    }

    //3d를 2d로 카메라 세팅
    public void PressButton() {

        //누름

        //판정
        if (btnType == ButtonType.jumpFence)
        {
        }
        else if (btnType == ButtonType.jumpWall)
        {
        }
        else if (btnType == ButtonType.slideWall)
        {
        }
        else { }
    }

    public enum ButtonType { 
    jumpFence, jumpWall, slideWall
    }
}


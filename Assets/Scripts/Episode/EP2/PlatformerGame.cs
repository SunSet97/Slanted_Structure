using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerGame : MonoBehaviour
{
    public ButtonType btnType;
    

    //판정이 되는 순간 버튼도 보이도록
    public void ActiveButton() { }

    public void PressButton() { 
        //누름
        //판정
        
    }

    public enum ButtonType { 
    jumpFence, jumpWall, slideWall
    }
}


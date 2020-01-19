using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Information_Scene : MonoBehaviour
{
    /*
     이 스크립트는 각 씬마다 '중복'되는 정보값을 일일이 스크립팅하는 짓을
     막기 위해 각 씬마다 <Scene_Information>오브젝트에 값입력하기만 하면 되는
     (타 스크립트에서 이 스크립트에서 정보값을 받도록 스크립트 짰다는 한에서)
     스크립팅 노가다 방지용 스크립트/오브젝트입니다.

    중복되는 값: 카메라 경계값+
    ※추후 추가되는 변수 값들은 밑의 양식으로 적어주십쇼※
    
    */

    [Header("카메라 경계값")]
    public float min_x = -1f;
    public float max_x = 5f;
    public float min_y = -2.0f;
    public float max_y = 1.3f;
    public float Z_Value = -10; //카메라 Z값 고정하기 위한 상수값

    /*나중에 인스턴스화 해야 될 듯. 각 씬마다 이 오브젝트 유무 판단하는 스크립트 필수*/
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//캐릭터 정보 구조체
public struct CharacterInfo
{
    public string name; //이름
    public CharacterManager char_mng; //캐릭터 매니져

    public CharacterInfo(string name, CharacterManager char_mng)
    {
        this.name = name;
        this.char_mng = char_mng;
    }
}

public class SceneInformation : MonoBehaviour
{
    /*
     이 스크립트는 각 씬마다 '중복'되는 정보값을 일일이 스크립팅하는 짓을
     막기 위해 각 씬마다 <SceneInformation>오브젝트에 값입력하기만 하면 되는
     (타 스크립트에서 이 스크립트에서 정보값을 받도록 스크립트 짰다는 한에서)
     스크립팅 노가다 방지용 스크립트/오브젝트입니다.

    중복되는 값: 카메라 경계값+
    ※추후 추가되는 변수 값들은 밑의 양식으로 적어주십쇼※
    
    */

    [Header("카메라 경계값")]
    public Camera cam;
    public float min_x = -1f;
    public float max_x = 5f;
    public float min_y = -2.0f;
    public float max_y = 1.3f;
    public float Z_Value = -10; //카메라 Z값 고정하기 위한 상수값

    [Header("카메라 각도")]
    public float Rot_x;
    public float Rot_y;
    public float Rot_z;

    [Header("플레이 방식")]
    public string playMethod = "Plt"; //2D 플랫포머(2D Platformer)=Plt, 쿼터뷰(Quarter view)=Qrt, 라인트레이서(Line tracer)=Line

    [Header("조이스틱")]
    public Joystick joyStick; //조이스틱

    [Header("캐릭터")]
    public CharacterInfo[] char_info = new CharacterInfo[]
    {
        new CharacterInfo("Rau",null),
        new CharacterInfo("Speat",null),
        new CharacterInfo("Oun",null)
    };

    //인스턴스화
    private static SceneInformation instance = null;
    public static SceneInformation instance_SceneInformation { get { return instance; } }

    private void Awake()
    {
        if (instance) { DestroyImmediate(this.gameObject); return; }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        //조이스틱 찾기
        if (!joyStick) joyStick = Joystick.FindObjectOfType<Joystick>();

        //캐릭터 매니저 찾아서 정보 저장
        //CharacterManager[] char_mngs = CharacterManager.FindObjectsOfType<CharacterManager>();
        //for (int i = 0; i < 3; i++)
        //    for (int j = 0; j < char_mngs.Length; j++)
        //        if (char_info[i].name == char_mngs[j].name)
        //            char_info[i].char_mng = char_mngs[j];
    }
}

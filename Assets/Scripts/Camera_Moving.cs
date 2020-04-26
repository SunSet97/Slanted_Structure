using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Moving : MonoBehaviour
{

    //Player 오브젝트 받는 변수
    public Transform Player;

    //Player의 Transform값 저장할 Transform변수
    //Transform Player_transform;

    //각 카메라와 Player의 Position값 저장할 Vector3 변수
    public Vector3 Camera_Position;
    public Vector3 Player_Position;
    public Quaternion Camera_Rotate;
    private NPCInteractor npcInteractor;


    // Start is called before the first frame update
    void Start()
    {
        npcInteractor = GameObject.Find("NPCManager").GetComponent<NPCInteractor>(); 
    }

    // Update is called once per frame
    void Update()
    {
        if (!Player)
        {
            //씬정보, Player 오브젝트 찾기
            //Player = SceneInformation.instance_SceneInformation.char_info[1].char_mng.transform;
            Player = npcInteractor.player.transform; 
        }
        else
        {
            Camera_Rotate = Camera.main.transform.rotation;
            //플레이어 위치 변수에 플레이어 위치 값 넣음.
            Player_Position = Player.GetComponent<Transform>().position;
            //메인 카메라의 위치를 플레이어 위치와 동일시함.
            Camera.main.transform.position = Player_Position;
            //지금 카메라의 위치를 카메라 위치변수에 넣음
            Camera_Position = Camera.main.transform.position;
            //카메라의 이동과 제한을 위한 함수에 매개변수로 카메라 위치변수 넣음.
            Follow_Player(Camera_Position,Camera_Rotate);
            //Player_transform.position = Player_Position;
        }



    }

    void Follow_Player(Vector3 position,Quaternion Camera_rotate)
    {
        //값변경을 위한 지역변수
        float position_storage;

        //카메라 각도 값 정보 Information_Scene오브젝트에서 받아오기
        //float rot_x = GameObject.Find("SceneInformation").GetComponent<SceneInformation>().Rot_x;
        //float rot_y = GameObject.Find("SceneInformation").GetComponent<SceneInformation>().Rot_y;
        //float rot_z = GameObject.Find("SceneInformation").GetComponent<SceneInformation>().Rot_z;
        Vector3 rot = DataController.instance_DataController.Rot;

        //카메라 경계값정보 Information_Scene오브젝트에서 받아오기
        float min_x = DataController.instance_DataController.min_x;
        float max_x = DataController.instance_DataController.max_x;
        float min_y = DataController.instance_DataController.min_y;
        float max_y = DataController.instance_DataController.max_y;
        float Z = DataController.instance_DataController.Z_Value;

        //카메라 위치 제한 설정
        position.x = Mathf.Clamp(position.x,min_x,max_x);
        position.y= Mathf.Clamp(position.y, min_y, max_y);
        Camera.main.transform.position = position;

        //입력 된 카메라 각도 설정
        Camera_rotate.eulerAngles = new Vector3(rot.x, rot.y, rot.z);
        Camera.main.transform.rotation = Camera_rotate;
        //카메라의 Z값 고정을 위한 If문
        if (position.z != Z)
        {
            position_storage = Z;
            position.z = position_storage;
            Camera.main.transform.position = position;
        }
    }

}

 
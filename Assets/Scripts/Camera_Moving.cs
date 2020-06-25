using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Moving : MonoBehaviour
{

    //캐릭터 오브젝트 받는 변수
    public Transform character;

    //각 카메라와 Player의 Position값 저장할 Vector3 변수
    private Vector3 charPos;
    private Vector3 camPos;
    private Quaternion camRot;


    // Update is called once per frame
    void Update()
    {
        if (DataController.instance_DataController.currentChar) character = DataController.instance_DataController.currentChar.gameObject.transform;

        if (character)
        {
            camRot = Camera.main.transform.rotation;
            //플레이어 위치 변수에 플레이어 위치 값 넣음.
            charPos = character.transform.position;
            //메인 카메라의 위치를 플레이어 위치와 동일시함.
            Camera.main.transform.position = new Vector3(charPos.x + DataController.instance_DataController.camDis_x, charPos.y+2f,charPos.z + DataController.instance_DataController.camDis_z);
            //지금 카메라의 위치를 카메라 위치변수에 넣음
            camPos = Camera.main.transform.position;
            //카메라의 이동과 제한을 위한 함수에 매개변수로 카메라 위치변수 넣음.
            Follow_Player(camPos, camRot);
            //Player_transform.position = Player_Position;
        }



    }

    void Follow_Player(Vector3 position,Quaternion Camera_rotate)
    {
        //값변경을 위한 지역변수
        float position_storage;

        //카메라 각도 값 정보 Information_Scene오브젝트에서 받아오기
        Vector3 rot = DataController.instance_DataController.rot;

        //카메라 경계값정보 Information_Scene오브젝트에서 받아오기
        float min_x = DataController.instance_DataController.min_x;
        float max_x = DataController.instance_DataController.max_x;
        float min_y = DataController.instance_DataController.min_y;
        float max_y = DataController.instance_DataController.max_y;
        float Z = DataController.instance_DataController.camDis_z;

        //카메라 위치 제한 설정
        //position.x = Mathf.Clamp(position.x,min_x,max_x);
        //position.y= Mathf.Clamp(position.y, min_y, max_y);
        Camera.main.transform.position = position;

        //입력 된 카메라 각도 설정
        Camera_rotate.eulerAngles = new Vector3(rot.x, rot.y, rot.z);
        Camera.main.transform.rotation = Camera_rotate;

        ////카메라의 Z값 고정을 위한 If문
        ////if (position.z != Z)
        //if (charPos.z - position.z != Z)
        //{
        //    position_storage = charPos.z - Z;
        //    position.z = position_storage;
        //    Camera.main.transform.position = position;
        //}
    }

}

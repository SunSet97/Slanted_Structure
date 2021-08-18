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

    // 카메라 무빙 경계
    private BoxCollider bound;
    private Vector3 minBound;
    private Vector3 maxBound;
    private float halfWidth;
    private float halfHeight;


    void Update()
    {
        if (DataController.instance_DataController.currentChar)
            character = DataController.instance_DataController.currentChar.gameObject.transform;

        if (character)
        {
            //플레이어 위치 변수에 플레이어 위치 값 넣음.
            charPos = character.transform.position;
            //메인 카메라의 위치를 플레이어 위치와 동일시함.
            Camera.main.transform.position = charPos + DataController.instance_DataController.camDis;
            //지금 카메라의 위치를 카메라 위치변수에 넣음
            camPos = Camera.main.transform.position;
            //카메라의 이동과 제한을 위한 함수에 매개변수로 카메라 위치변수 넣음.
            Follow_Player(camPos);
            //Player_transform.position = Player_Position;

        }
    }

    void Follow_Player(Vector3 position)
    {
        //카메라 경계값정보 Information_Scene오브젝트에서 받아오기
        float min_x = DataController.instance_DataController.min_x;
        float max_x = DataController.instance_DataController.max_x;
        float min_y = DataController.instance_DataController.min_y;
        float max_y = DataController.instance_DataController.max_y;

        //카메라 위치 제한 설정
        //position.x = Mathf.Clamp(position.x,min_x,max_x);
        //position.y= Mathf.Clamp(position.y, min_y, max_y);

        if (DataController.instance_DataController.currentMap.cameraBound) // 카메라 경계 설정 시
        {
            bound = DataController.instance_DataController.currentMap.cameraBound;
            minBound = bound.bounds.min;
            maxBound = bound.bounds.max;
            halfHeight = GetComponent<Camera>().orthographicSize;
            halfWidth = halfHeight * Screen.width / Screen.height;
            float clampedX = Mathf.Clamp(transform.position.x, minBound.x + halfWidth, maxBound.x - halfWidth);
            float clampedY = Mathf.Clamp(transform.position.y, minBound.y + halfHeight, maxBound.y - halfHeight); ;
            //float clampedZ;
            transform.position = new Vector3(clampedX, clampedY, transform.position.z);

        }
        else
        {
            Camera.main.transform.position = position + DataController.instance_DataController.camDis;
        }

        //입력 된 카메라 각도 설정
        Camera.main.transform.rotation = Quaternion.Euler(DataController.instance_DataController.rot);

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

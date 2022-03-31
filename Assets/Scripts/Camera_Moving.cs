using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Moving : MonoBehaviour
{
    //캐릭터 오브젝트 받는 변수
    public Transform character;

    private CustomEnum.CameraViewType viewType;
    

    // 카메라 무빙 경계
    private BoxCollider bound;
    private Vector3 minBound;
    private Vector3 maxBound;
    private float halfWidth;
    private float halfHeight;

    public void Initialize(CustomEnum.CameraViewType viewType)
    {
        Debug.Log("초기화초기화초기화" + viewType);
        this.viewType = viewType;
        character = DataController.instance_DataController.GetCharacter(MapData.Character.Main).transform;
        Debug.Log(character);
    }
    void Update()
    {
        var cam = Camera.main;
        Debug.Log(character);
        if (character)
        {
            if (cam.orthographic) cam.orthographicSize = DataController.instance_DataController.orthgraphic_Size;

            if (viewType.Equals(CustomEnum.CameraViewType.FixedView))
            {
                cam.transform.position = DataController.instance_DataController.currentMap.transform.position +
                                         DataController.instance_DataController.camDis;
            }
            else if (viewType.Equals(CustomEnum.CameraViewType.FollowCharacter))
            {
                cam.transform.position = character.position + DataController.instance_DataController.camDis;
            }
            Follow_Player(cam.transform.position);
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

        //입력 된 카메라 각도 설정
        Camera.main.transform.rotation = Quaternion.Euler(DataController.instance_DataController.camRot);

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Moving : MonoBehaviour
{

    //Player 오브젝트 받는 변수
    public GameObject Player;

    //Player의 Transform값 저장할 Transform변수
    Transform Player_transform;

    //각 카메라와 Player의 Position값 저장할 Vector3 변수
    Vector3 Camera_Position;
    Vector3 Player_Position;


    

    // Start is called before the first frame update
    void Start()
    {
        //씬정보, Player 오브젝트 찾기
       
        Player = GameObject.Find("Player");
       
        //Player의 Transform 속성 객체화
        Player_transform = Player.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        //플레이어 위치 변수에 플레이어 위치 값 넣음.
        Player_Position = Player_transform.position;
        //메인 카메라의 위치를 플레이어 위치와 동일시함.
        Camera.main.transform.position = Player_transform.position;
        //지금 카메라의 위치를 카메라 위치변수에 넣음
        Camera_Position = Camera.main.transform.position;
        //카메라의 이동과 제한을 위한 함수에 매개변수로 카메라 위치변수 넣음.
        Follow_Player(Camera_Position);
        Player_transform.position = Player_Position;

    }

    void Follow_Player(Vector3 position)
    {
        //값변경을 위한 지역변수
        float position_storage;

        //카메라 경계값정보 Information_Scene오브젝트에서 받아오기
        float min_x = GameObject.Find("Information_Scene").GetComponent<Information_Scene>().min_x;
        float max_x = GameObject.Find("Information_Scene").GetComponent<Information_Scene>().max_x;
        float min_y = GameObject.Find("Information_Scene").GetComponent<Information_Scene>().min_y;
        float max_y = GameObject.Find("Information_Scene").GetComponent<Information_Scene>().max_y;
        float Z = GameObject.Find("Information_Scene").GetComponent<Information_Scene>().Z_Value;

        //카메라 위치 제한 설정
        position.x = Mathf.Clamp(position.x,min_x,max_x);
        position.y= Mathf.Clamp(position.y, min_y, max_y);
        Camera.main.transform.position = position;

        //카메라의 Z값 고정을 위한 If문
        if (position.z != Z)
        {
            position_storage = Z;
            position.z = position_storage;
            Camera.main.transform.position = position;
        }
    }

}

 
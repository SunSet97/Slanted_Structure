using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Moving : MonoBehaviour
{
    public GameObject Player;
    //카메라 Transform속성 저장할 변수와 Transform의 position값 저장할 변수
    Transform Player_transform;
    Vector3 Position_;
    Vector3 Position_player;
    public const float Z_Value=-10;

    //카메라 경계값
    [Header("카메라 경계값")]
    public float min_x=-1f;
    public float max_x=5f;
    public float min_y=-2.0f;
    public float max_y=1.3f;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("작동");
        Player=GameObject.Find("Player");
        //플레이어와 카메라 오브젝트의 Transform 속성 객체화,각 position값 객체화
        Player_transform = Player.GetComponent<Transform>();
        
    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 Position;
        Position_player = Player_transform.position;
        Camera.main.transform.position = Player_transform.position;
        Position_ = Camera.main.transform.position;
        Follow_Player(Position_);
        Player_transform.position=Position_player;

    }

    void Follow_Player(Vector3 position)
    {
        float position_storage;
        if (position.z != Z_Value)
        {
            position_storage = Z_Value;
            position.z = position_storage;
            Camera.main.transform.position = position;
        }

        //카메라 위치가 일정 위치에서 벗어날 때 멈추기
        //X값
        if (position.x > max_x)
        {
            Number_x(position, max_x);
            if (position.y > max_y)
            {
                Number_y(position, max_y);
            }
            else if (Position_.y < min_y)
            {
                Number_y(position, min_y);
            }
            Number_y(position, position.y);
        }
        else if (position.x < min_x)
        {
            Number_x(position, min_x);
            if (position.y > max_y)
            {
                Number_y(position, max_y);
            }
            else if (Position_.y < min_y)
            {
                Number_y(position, min_y);
            }
            Number_y(position, position.y);
        }
        else if(position.x <= max_x&& position.x >= min_x)
        {
            if (position.y > max_y)
            {
                Number_y(position, max_y);
            }
            else if (Position_.y < min_y)
            {
                Number_y(position, min_y);
            }

        }


        //Y값
        if (position.y > max_y)
        {
            Number_y(position, max_y);
            if (position.x > max_x)
            {
                Number_x(position, max_x);
            }
            else if (position.x < min_x)
            {
                Number_x(position, min_x);
            }
            Number_x(position, position.x);
        }
        else if (Position_.y < min_y)
        {
            Number_y(position, min_y);
            if (position.x > max_x)
            {
                Number_x(position, max_x);
            }
            else if (position.x < min_x)
            {
                Number_x(position, min_x);
            }
            Number_x(position, position.x);
        }
        else if (position.y <= max_y && position.y >= min_y)
        {
            if (position.x > max_x)
            {
                Number_x(position, max_x);
            }
            else if (Position_.x < min_x)
            {
                Number_x(position, min_x);
            }
        }
            
        Debug.Log("작동");
    }

    void Number_x(Vector3 position,float number)
    {
        float position__;
        position__ = number;
        position.x = position__;
        Camera.main.transform.position = position;
    }
    void Number_y(Vector3 position, float number)
    {
        float position__;
        position__=number;
        position.y = position__;
        Camera.main.transform.position = position;
    }

}

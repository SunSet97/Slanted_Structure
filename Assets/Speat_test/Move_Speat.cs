using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move_Speat : MonoBehaviour
{
    public int speed = 6; //이동 속도
    public Joystick joy; //조이스틱
    Vector3 joy_v; //조이스틱 벡터

    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Moving();
    }
    void Moving()
    {
        joy_v.Set(joy.Horizontal * speed * Time.fixedDeltaTime, 0, 0); //조이스틱의 horizontal값으로 벡터 set
        gameObject.transform.position += joy_v;

       // Debug.Log(joy_v.x);
        if (joy_v.x < 0 && transform.eulerAngles.y != 90) //방향 전환(왼쪽)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
        }
        else if (joy_v.x > 0 && transform.eulerAngles.y != -90) //방향 전환(오른쪽)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    public float xSpeed = -50;
    public float max = 50;
    public float min = -50;
    public float angle = 0; // 앵글 확인하기 위한 변수 

    bool isPossibleMove = true;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //각도 구현

        if(isPossibleMove == true)
        {
            angle += xSpeed * Time.deltaTime;

            if (angle >= max)
            {
                transform.eulerAngles = new Vector3(max, transform.eulerAngles.y, transform.eulerAngles.z);
                xSpeed = -50;
            }

            if (angle <= min)
            {
                transform.eulerAngles = new Vector3(310, transform.eulerAngles.y, transform.eulerAngles.z);
                xSpeed = 50;
            }

            transform.Rotate(xSpeed * Time.deltaTime, 0, 0);
        }

        if (Input.GetMouseButtonDown(0))
        {
            isPossibleMove = false;
        }

    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Speat : MonoBehaviour
{
    public Transform speat; //speat의 트랜스폼
    public float z_offset = 8; //플레이어와 카메라의 거리
    Vector3 y_offset = new Vector3(0, 1, 0); //y 1만큼 올려줘야 위층 아래층 한칸씩 보임.

    void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, speat.position + y_offset, 5 *Time.fixedDeltaTime);
        transform.position = new Vector3(transform.position.x, transform.position.y, -z_offset);
    }
}

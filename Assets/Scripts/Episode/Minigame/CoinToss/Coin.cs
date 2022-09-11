using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public Vector3 dir; // 나가는 방향
    public float force; // 던져지는 힘
    private float height; // 시작 높이

    void Start()
    {
        Rigidbody myRigidbody = GetComponent<Rigidbody>();
        myRigidbody.AddForce(dir * force); // 설정된 방향에 입력된 힘 가하기
        height = transform.position.y; // 바닥과의 거리 계산을 위해 시작 높이 설정
        Destroy(this.gameObject, 3f); // 3초 후 자동 제거
    }
    
    void Update()
    {
        if (transform.position.y < height - 0.2f) return; // 동전이 바닥에 거의 닿을 시 회전 중지
        else transform.Rotate(transform.forward, force / 5); // 동전이 공중에 있을 때 회전
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.name == "water_upper")
        {
            FindObjectOfType<TossCoin>().isPass = true; // 미니게임 통과
            Destroy(this.gameObject); // 동전 제거
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TossCoin : MonoBehaviour
{
    public GameObject coin;
    public Transform tossPoint;
    private Vector3 tossDir;
    float time;
    
    void Update()
    {
        tossDir = (tossPoint.position - transform.position).normalized;

        if (Input.GetButtonDown("Fire1"))
        {
            //던질 동전 만들기
            GameObject tossedCoin = Instantiate(coin, transform.position, Quaternion.identity);
            tossedCoin.GetComponent<Coin>().dir = tossDir;
        }

        //각도 회전
        time += Time.deltaTime;
        transform.eulerAngles = Vector3.forward * (Mathf.Sin(time) * 45 + 45);
    }
}

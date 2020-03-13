using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickPocket_phw : MonoBehaviour
{
    public float moveSpeed;
    public float distanceBetwwenRauAndPcikPocket; // 겜 시작할 때 라우와 떨어진 거리.
    public Transform rau;

    void Start()
    {
        transform.position = new Vector3(rau.position.x, rau.position.y, rau.position.z + distanceBetwwenRauAndPcikPocket);
    }

    void Update()
    {
        transform.Translate(new Vector3(0,0,moveSpeed*Time.deltaTime));
    }
}

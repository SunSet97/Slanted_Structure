using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour {
    public Transform target;
    public int offsetY; // 추가

    void Update ()
    {
        transform.LookAt(new Vector3(target.position.x, target.position.y + offsetY, target.position.z - 10)); // 추가
        // transform.LookAt(target); 원래 있던 코드
    }
}

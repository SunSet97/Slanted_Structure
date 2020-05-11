using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pigeon : MonoBehaviour
{
    public Transform rau;
    public Transform[] pigeons;
    public float moveSpeed;
    public float height; // 비둘기가 라우 위쪽으로 얼마나 높게 날 것인지.s
    private bool isCollision = false;

    void Start() {
        pigeons = GetComponentsInChildren<Transform>();
    }

    void Update() {
        
        if (isCollision) { // 라우 머리 위를 향해 날라감.
            Vector3 flyingDir = (rau.position - transform.position + new Vector3(-15, height, 20)).normalized; //너무 길어서 보기 좋게 한줄 뺌

            for (int i = 1; i < pigeons.Length; i++) {
                float ran = Random.Range(-0.1f, 1f); //비둘기들 마다 다르게 날아가도록 함
                pigeons[i].Translate(flyingDir * ran * moveSpeed * Time.deltaTime); //각각의 비둘기들이 날아가도록 함

                if (pigeons[i].position.y >= height) { Destroy(this.gameObject); } //한 마리라도 날아가는 높이를 넘어가면 제거
            }

        }        
    }

    private void OnTriggerEnter(Collider other) {
        if (other.name == "Rau") {
            isCollision = true;
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pigeon_phw : MonoBehaviour
{

    // public Camera maincam;
    //public Collider mycollider;
    public Rau_phw rau;
    public float moveSpeed;
    public float height; // 비둘기가 라우 위쪽으로 얼마나 높게 날 것인지.
    float screenLeft;
    float screenRight;


    void Start() {
        //screenLeft = maincam.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
        // screenRight = maincam.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
    }

    void Update() {

        if (rau.detectAround) { // 라우 머리 위를 향해 날라감.
            Debug.Log("비둘기 주위를 돌며 지나감.");
            Debug.Log("conflictwithCrowd  " + rau.conflictWithCrowd);
            transform.Translate((rau.transform.position - transform.position + new Vector3(-15,height,20)).normalized * moveSpeed * Time.deltaTime);
        }

        if ((int)transform.position.y == 7) {
            gameObject.SetActive(false);
        }
        
        


    }

    /*public void pigeonMove() {
        transform.Translate((rau.transform.position - transform.position + new Vector3(-10, height, 10)).normalized * moveSpeed * Time.deltaTime);
    }*/


}

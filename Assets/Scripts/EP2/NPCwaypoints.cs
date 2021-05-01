using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCwaypoints : MonoBehaviour
{
    public Transform[] pointPos;

    public int[] branchIndexList;
    public float speed;

    public int pointIndex = 0;
    public int branchCheckIndex = 0;
    public bool isNpcMoving = false;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < pointPos.Length; i++) {
            pointPos[i].position = new Vector3(pointPos[i].position.x, transform.position.y, pointPos[i].position.z);
        }

        // 처음 위치로
        transform.position = pointPos[0].transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        MoveToPoint();
    }

    public void MoveToPoint() {

        if (isNpcMoving && transform.position == pointPos[pointIndex].transform.position)
        {
            pointIndex++;
            if (pointIndex < branchIndexList[branchCheckIndex] && branchCheckIndex < branchIndexList.Length)
            {
                StopMoving();
                branchCheckIndex++;
            }

        }
        else if (isNpcMoving && transform.position != pointPos[pointIndex].transform.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, pointPos[pointIndex].transform.position, speed * Time.deltaTime);
            transform.LookAt(pointPos[pointIndex]);

            if (pointIndex == pointPos.Length - 1)
            {
                StopMoving();
            }

        }
        else if (!isNpcMoving)
        {
            GetComponent<Animator>().SetBool("Dash", false);
        }


        /*
        if (isNpcMoving)
        {

            transform.position = Vector3.MoveTowards(transform.position, pointPos[pointIndex].transform.position, speed * Time.deltaTime);
            transform.LookAt(pointPos[pointIndex]);

            if (pointIndex == pointPos.Length - 1)
            {
                StopMoving();
                print("끝");
            }

        }
        */

    }


    public void StopMoving()
    {
        GetComponent<Animator>().SetBool("Walk", false);
        //GetComponent<Animator>().SetFloat("Speed", 0f);
        isNpcMoving = false;
    }

    public void StartMoving()
    {
        GetComponent<Animator>().SetBool("Walk", true);
        //GetComponent<Animator>().SetFloat("Speed", 0.3f);
        isNpcMoving = true;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct PointData
{
    public Transform transform;
    public bool isStop;
    public UnityAction pointEvent; 
}

public class NPCwaypoints : MonoBehaviour
{
    public PointData[] point;

    public float speed;

    public int pointIndex = 0;
    public int branchCheckIndex = 0;
    public bool isMoving = false;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < point.Length; i++)
            point[i].transform.position.Set(point[i].transform.position.x, transform.position.y, point[i].transform.position.z);

        // 처음 위치로
        transform.position = point[0].transform.position;

    }
    public IEnumerator MoveToPoint()
    {
        SetMoving(true);
        while (isMoving)
        {
            //이동 완료
            if (transform.position == point[pointIndex].transform.position)
            {
                if (point[pointIndex].isStop)
                {
                    SetMoving(false);
                    if (point[pointIndex].pointEvent != null)
                    {
                        point[pointIndex].pointEvent();
                        branchCheckIndex++;
                    }
                }
                transform.LookAt(point[++pointIndex].transform);
            }
            //이동 중
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, point[pointIndex].transform.position, speed * Time.deltaTime);

            }
            yield return null;
        }
    }

    public void SetPointEvent(UnityAction pointAction, int index)
    {
        point[index].pointEvent = pointAction;
    }

    public void SetMoving(bool isMove)
    {
        if (isMove)
        {
            GetComponent<Animator>().SetFloat("Speed", 1f);
        }
        else
        {
            GetComponent<Animator>().SetFloat("Speed", 0f);
        }

        //GetComponent<Animator>().SetBool("Walk", isMove);
        isMoving = isMove;
    }
    public void StartMoving()
    {
        StartCoroutine(MoveToPoint());
    }

}

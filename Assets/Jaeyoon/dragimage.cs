using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class dragimage : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Sprite[] imageChange;
    //안인지 밖인지 판정하는 bool
    public bool positionDecide;
    public int curIndex;
    public int standard_Cut;
    GraphicRaycaster dalgonaRaycaster;

    Vector3 startPos;

    public void Start()
    {
        //현재 sprite의 태그를 dalgonaboundary로 바꿔줌.
        transform.GetChild(curIndex).gameObject.tag = "DalgonaBoundary";
        transform.GetChild(curIndex).GetComponent<Image>().raycastTarget = true;
        dalgonaRaycaster = GetComponent<GraphicRaycaster>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {

        // 시작 vec
        startPos = eventData.position;
        List<RaycastResult> results = new List<RaycastResult>();
        dalgonaRaycaster.Raycast(eventData, results);
      
        if(results.Count == 0) { 
            Debug.Log("나감");
            if (positionDecide)
            {
                if (CheckDistance((Vector2)eventData.position))
                {
                    ChangeNextDalgona();
                    startPos = Vector3.zero;
                }
            }
            positionDecide = false; 
        }
        foreach (RaycastResult result in results)
            {
                //RaycastHit hit = hits[i];
                //??? -> 안
                //hit.collider.gameObject.tag.Equals("DalgonaBoundary")
                if (result.gameObject.tag.Equals("DalgonaBoundary"))
                {
                    //DalgonaBoundary 내부에 있는 경우 -> 안
                  //  if (!positionDecide)
                    {
                        Debug.Log("안으로");
                        positionDecide = true;
                        //거리 계산 (startpoint부터 마우스의 위치까지)
                    }
                    break;
                }
                //??? -> 밖
                else
                {
                    //시작점이 내부
                    Debug.Log("나감"); 
                    positionDecide = false;
                    //if (positionDecide)
                    //{
                    //    Debug.Log("나감");
                    //    positionDecide = false;
                    //}

            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        //squareImageChange[0].position = eventData.position;
        //squareImageChange[1].gameObject.SetActive(false);
        //Ray ray = Camera.main.ScreenPointToRay(eventData.pointerCurrentRaycast.screenPosition);
        //bool isHitted = Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue);
        //if (isHitted) {
        //    Gizmos.DrawRay(ray);
        //    if (raycastHit.collider.gameObject.tag == "DalgonaBoundary")
        //    {}
        //}
        //Ray ray = new Ray(eventData.position, Camera.main.transform.forward);
        //RaycastHit[] hits = Physics.RaycastAll(ray, float.MaxValue);

        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        Debug.DrawRay(eventData.position, ray.direction * 40f, Color.black, 2f);
        //Debug.Log("ray");

        List<RaycastResult> results = new List<RaycastResult>();
        dalgonaRaycaster.Raycast(eventData, results);
        if(results.Count == 0) {
            if (positionDecide)
            {
                if (CheckDistance((Vector2)eventData.position))
                {
                    ChangeNextDalgona();
                    startPos = Vector3.zero;
                }
            }

            positionDecide = false;
            return; 
        }
        foreach (RaycastResult result in results)
        {
            //RaycastHit hit = hits[i];
            //??? -> 안
            //hit.collider.gameObject.tag.Equals("DalgonaBoundary")
            if (result.gameObject.tag.Equals("DalgonaBoundary"))
            {
                //DalgonaBoundary 내부에 있는 경우 -> 안
                if (positionDecide)
                {
                    //거리체크
                    if (CheckDistance((Vector2)eventData.position))
                    {
                        ChangeNextDalgona();
                        startPos = Vector3.zero;
                    }
                }
                //DalgonaBoundary 외부에 있는 경우 -> 안
                else
                {
                    Debug.Log("안으로");
                    //거리 계산 (startpoint부터 마우스의 위치까지)
                    startPos = eventData.position;
                }
                positionDecide = true;
                break;
            }
            //??? -> 밖
            //image가 Dalgona_Square로
            else
            {
                //시작점이 내부
                if (positionDecide)
                {
                    Debug.Log("나감");
                    positionDecide = false;
                    if (CheckDistance((Vector2)eventData.position))
                    {
                        Debug.Log(results.Count);
                        Debug.Log(startPos);
                        ChangeNextDalgona();
                        startPos = Vector3.zero;
                    }
                }
            }
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log(CheckDistance(eventData.position));
        if (CheckDistance(eventData.position) && positionDecide)
        {
            ChangeNextDalgona();
            startPos = Vector3.zero;
        }
    }

    private bool CheckDistance(Vector2 currentposition)
    {
        if (startPos == Vector3.zero) return false;
        float dist = Vector2.Distance(currentposition, startPos);
        if (dist > standard_Cut)
        {
            return true;
        }
        else { return false; }
    }
    //다음의 sprite로 바꿈
    void ChangeNextDalgona()
    {
        //imageChange[] 다음의 sprite로 바뀜
        curIndex = Array.IndexOf(imageChange, transform.GetComponent<Image>().sprite);
        Debug.Log(curIndex);
        if (imageChange.Length <= curIndex) return;
        //square~배열에서 현재의 sprite를 찾고 다음 sprite로 변경
        //collider check 못하게 / 이미지 변경
        transform.GetChild(curIndex).GetComponent<Image>().raycastTarget = false;
        transform.GetChild(curIndex).gameObject.tag = "Untagged";
        transform.GetComponent<Image>().sprite = imageChange[curIndex + 1];

        if (curIndex + 1 < imageChange.Length - 1)
        {

            transform.GetChild(curIndex + 1).GetComponent<Image>().raycastTarget = true;
            transform.GetChild(curIndex + 1).gameObject.tag = "DalgonaBoundary";
        }
        else
        {
            //게임 종료"? 다음 달고나?
        }
    }

}

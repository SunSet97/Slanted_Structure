using System;
using System.Collections.Generic;
using System.Security.Authentication.ExtendedProtection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DalgonaDrager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Sprite[] imageChange;
    public int standardCut;

    private GraphicRaycaster dalgonaRaycaster;

    private bool isPointIn;
    private Vector2 dragBeginPos;
    private bool isDrag;

    [NonSerialized]
    public bool isEnd;


    public void Init()
    {
        isEnd = false;
        gameObject.SetActive(true);
        dalgonaRaycaster = GetComponentInParent<GraphicRaycaster>();
        for (var idx = 0; idx < transform.childCount; idx++)
        {
            var child = transform.GetChild(idx);
            child.gameObject.tag = "Untagged";
            child.GetComponent<Image>().raycastTarget = false;
        }

        var dalgonaImage = transform.GetComponent<Image>();
        var initChild = transform.GetChild(0);
        initChild.GetComponent<Image>().raycastTarget = true;
        initChild.gameObject.tag = "DalgonaBoundary";
        dalgonaImage.sprite = imageChange[0];
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDrag = true;
        dragBeginPos = eventData.position;
        List<RaycastResult> results = new List<RaycastResult>();
        dalgonaRaycaster.Raycast(eventData, results);

        if (results.Count == 0)
        {
            if (isPointIn)
            {
                if (CheckDistance(eventData.position))
                {
                    ChangeNextDalgona();
                }
            }

            isPointIn = false;
        }

        var result = results.Find(raycastResult => raycastResult.gameObject.tag.Equals("DalgonaBoundary"));

        if (result.isValid)
        {
            isPointIn = true;
        }
        else
        {
            isPointIn = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        dalgonaRaycaster.Raycast(eventData, results);

        var result = results.Find(raycastResult => raycastResult.gameObject.tag.Equals("DalgonaBoundary"));

        if (result.isValid)
        {
            if (isPointIn)
            {
                if (CheckDistance(eventData.position))
                {
                    ChangeNextDalgona();
                }
            }
            else
            {
                dragBeginPos = eventData.position;
                isPointIn = true;
            }
        }
        else if (isPointIn)
        {
            if (CheckDistance(eventData.position))
            {
                ChangeNextDalgona();
            }
            isPointIn = false;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("측정 길이: " + CheckDistance(eventData.position) + " " +
                  Vector2.Distance(eventData.position, dragBeginPos) + ":" + standardCut);
        if (CheckDistance(eventData.position) && isPointIn)
        {
            ChangeNextDalgona();
        }

        isDrag = false;
    }

    private bool CheckDistance(Vector2 currentPos)
    {
        if (isDrag)
        {
            float dist = Vector2.Distance(currentPos, dragBeginPos);
            Debug.Log(dist);
            if (dist > standardCut)
            {
                return true;
            }
        }

        return false;
    }
    
    
    private void ChangeNextDalgona()
    {
        isPointIn = false;
        isDrag = false;
        dragBeginPos = Vector3.zero;

        var dalgonaImage = transform.GetComponent<Image>();
        var curIndex = Array.IndexOf(imageChange, dalgonaImage.sprite);

        var child = transform.GetChild(curIndex);
        child.GetComponent<Image>().raycastTarget = false;
        child.gameObject.tag = "Untagged";

        if (curIndex + 1 < imageChange.Length && curIndex + 1 < transform.childCount)
        {
            var nextChild = transform.GetChild(curIndex + 1);
            nextChild.GetComponent<Image>().raycastTarget = true;
            nextChild.gameObject.tag = "DalgonaBoundary";
            dalgonaImage.sprite = imageChange[curIndex + 1];
        }
        else
        {
            dalgonaImage.sprite = imageChange[curIndex + 1];
            isEnd = true;
            gameObject.SetActive(false);
        }
    }
}

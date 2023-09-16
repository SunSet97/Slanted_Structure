using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Episode.EP1.DalgonaGame
{
    public class DalgonaDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Sprite[] imageChange;
        public int standardCut;

        private Action onEndAction;
        private GraphicRaycaster dalgonaRaycaster;
        private bool isPointIn;
        private Vector2 dragBeginPos;
        private List<RaycastResult> results;

        public void Init(Action onDragEndAction)
        {
            onEndAction = onDragEndAction;
            dalgonaRaycaster = GetComponentInParent<GraphicRaycaster>();
            results = new List<RaycastResult>();
            
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

        public void Play()
        {
            gameObject.SetActive(true);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragBeginPos = eventData.position;
            results.Clear();
            dalgonaRaycaster.Raycast(eventData, results);

            if (results.Count == 0)
            {
                if (isPointIn && CheckDistance(eventData.position))
                {
                    ChangeNextStep();
                }

                isPointIn = false;
            }

            var result = results.Find(raycastResult => raycastResult.gameObject.tag.Equals("DalgonaBoundary"));

            isPointIn = result.isValid;
        }

        public void OnDrag(PointerEventData eventData)
        {
            results.Clear();
            dalgonaRaycaster.Raycast(eventData, results);

            var result = results.Find(raycastResult => raycastResult.gameObject.tag.Equals("DalgonaBoundary"));

            if (result.isValid)
            {
                if (isPointIn)
                {
                    if (CheckDistance(eventData.position))
                    {
                        ChangeNextStep();
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
                    ChangeNextStep();
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
                ChangeNextStep();
            }
        }

        private bool CheckDistance(Vector2 currentPos)
        {
            float dist = Vector2.Distance(currentPos, dragBeginPos);
            Debug.Log(dist);
            if (dist > standardCut)
            {
                return true;
            }

            return false;
        }

        private void ChangeNextStep()
        {
            isPointIn = false;
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
                gameObject.SetActive(false);
                onEndAction();
            }
        }
    }
}
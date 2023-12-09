using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Episode.EP1.DalgonaGame
{
    public class DalgonaDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [FormerlySerializedAs("imageChange")] [SerializeField]
        private Sprite[] dalgonaSprites;
        [SerializeField] private Sprite[] failSprites;
        
        [SerializeField] private Image[] dragZone;
        [SerializeField] private Image deadZone;
        
        public int standardCut;

        private Action<bool> onEndGameAction;
        private GraphicRaycaster dalgonaRaycaster;
        private Image dalgonaImage;

        private int index;
        private bool wasPointIn;
        private Vector2 dragBeginPos;

        private readonly List<RaycastResult> results = new List<RaycastResult>();

        public void Init(Action<bool> onEndGame)
        {
            onEndGameAction = onEndGame;
            dalgonaRaycaster = GetComponentInParent<GraphicRaycaster>();
            dalgonaImage = transform.GetComponent<Image>();
            deadZone.alphaHitTestMinimumThreshold = .1f;

            for (var idx = 0; idx < dragZone.Length; idx++)
            {
                SetChildImageRayCast(false, idx);
            }
        }

        public void Play()
        {
            gameObject.SetActive(true);

            SetChildImageRayCast(true, 0);
            dalgonaImage.sprite = dalgonaSprites[0];
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            results.Clear();
            dragBeginPos = eventData.position;
            dalgonaRaycaster.Raycast(eventData, results);

            if (results.Count == 0)
            {
                if (wasPointIn)
                {
                    CheckClear(eventData.position);
                }

                wasPointIn = false;
            }
            else
            {
                if (results.Any(item => item.isValid && item.gameObject == deadZone.gameObject))
                {
                    StartCoroutine(EndPlay(false));
                    return;
                }

                var result = results.Find(raycastResult => raycastResult.gameObject.tag.Equals("DalgonaBoundary"));

                wasPointIn = result.isValid;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            Debug.LogWarning($"Length - {Vector2.Distance(eventData.position, dragBeginPos)}, {standardCut}");
            results.Clear();
            dalgonaRaycaster.Raycast(eventData, results);

            if (results.Any(item => item.isValid && item.gameObject == deadZone.gameObject))
            {
                StartCoroutine(EndPlay(false));
                return;
            }
            
            var result = results.Find(raycastResult => raycastResult.gameObject.tag.Equals("DalgonaBoundary"));

            if (result.isValid)
            {
                if (wasPointIn)
                {
                    CheckClear(eventData.position);
                }
                else
                {
                    dragBeginPos = eventData.position;
                    wasPointIn = true;
                }
            }
            else if (wasPointIn)
            {
                wasPointIn = false;
                
                CheckClear(eventData.position);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (results.Any(item => item.isValid && item.gameObject == deadZone.gameObject))
            {
                StartCoroutine(EndPlay(false));
                return;
            }
            
            Debug.Log("측정 길이: " + CheckDistance(eventData.position) + " " +
                      Vector2.Distance(eventData.position, dragBeginPos) + ":" + standardCut);
            if (wasPointIn)
            {
                CheckClear(eventData.position);
            }
        }

        private void CheckClear(Vector2 point)
        {
            if (CheckDistance(point))
            {
                ChangeNextStep();
            }
        }

        private bool CheckDistance(Vector2 currentPos)
        {
            var dist = Vector2.Distance(currentPos, dragBeginPos);
            Debug.Log(dist);
            if (dist > standardCut)
            {
                return true;
            }

            return false;
        }

        private void ChangeNextStep()
        {
            wasPointIn = false;
            dragBeginPos = Vector3.zero;

            var curIndex = Array.IndexOf(dalgonaSprites, dalgonaImage.sprite);
            var nextIndex = curIndex + 1;

            SetChildImageRayCast(false, curIndex);
            dalgonaImage.sprite = dalgonaSprites[nextIndex];
            
            if (!IsLast())
            {
                SetChildImageRayCast(true, nextIndex);
            }
            else
            {
                StartCoroutine(EndPlay(true));
            }
        }

        private bool IsLast()
        {
            var nextIndex = Array.IndexOf(dalgonaSprites, dalgonaImage.sprite) + 1;
            return nextIndex >= dalgonaSprites.Length || nextIndex >= dragZone.Length;
        }

        private IEnumerator EndPlay(bool isSuccess)
        {
            if (!isSuccess)
            {
                dalgonaImage.sprite = failSprites[index];
            }
            
            yield return new WaitForSeconds(1f);
            gameObject.SetActive(false);
            onEndGameAction(isSuccess);   
        }

        private void SetChildImageRayCast(bool isActive, int zoneIndex)
        {
            if (isActive)
            {
                index = zoneIndex;
                dragZone[zoneIndex].raycastTarget = true;
                dragZone[zoneIndex].gameObject.tag = "DalgonaBoundary";
            }
            else
            {
                dragZone[zoneIndex].raycastTarget = false;
                dragZone[zoneIndex].gameObject.tag = "Untagged";
            }
        }
    }
}
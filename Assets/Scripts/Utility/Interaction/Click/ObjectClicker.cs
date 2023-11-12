using System;
using System.Collections.Generic;
using UnityEngine;
using Utility.Core;

namespace Utility.Interaction.Click
{
    public class ObjectClicker : MonoBehaviour
    {
        public static ObjectClicker Instance { get; private set; }

        // 검색하는 layer
        private int layerMask;

        /// <summary>
        /// true인 경우, 자체적인 Touch 사용
        /// 맵 초기화마다 변경
        /// </summary>
        [NonSerialized]
        public bool IsCustomUse;

        private readonly List<IClickable> clickableList = new List<IClickable>();
        private readonly RaycastHit[] results = new RaycastHit[5];
        
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            layerMask = 1 << LayerMask.NameToLayer("ClickObject") | 1 << LayerMask.NameToLayer("OnlyPlayerCheck");
        }

        private void Update()
        {
            if (!IsCustomUse)
            {
                OnTouchDisplay();
            }
        }

        public void UpdateClick(IClickable clickable, bool isActive)
        {
            if (isActive)
            {
                AddClick(clickable);
            }
            else
            {
                RemoveClick(clickable);
            }
        }

        private void AddClick(IClickable clickable)
        {
            if (clickableList.Contains(clickable))
            {
                return;
            }
            
            if (!gameObject.activeSelf)
            {
                Debug.Log($"클리커 활성화");
            }
            
            clickableList.Add(clickable);
            gameObject.SetActive(true);
        }

        private void RemoveClick(IClickable clickable)
        {
            if (!clickableList.Contains(clickable))
            {
                return;
            }
            clickableList.Remove(clickable);
            if (clickableList.Count == 0)
            {
                gameObject.SetActive(false);
                Debug.Log($"클리커 비활성화");
            }
        }

        private void OnTouchDisplay()
        {
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            var rect = JoystickController.Instance.Joystick.GetComponent<RectTransform>();

            // if (Input.mousePosition.x / Screen.currentResolution.width <= rect.anchorMax.x
            //     && Input.mousePosition.y / Screen.currentResolution.height <= rect.anchorMax.y)
            // {
            //     return;
            // }
            Debug.Log("클릭");
            var ray = DataController.Instance.Cam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 20f, Color.red, 5f);
            var count = Physics.RaycastNonAlloc(ray, results, Mathf.Infinity, layerMask);
            for (var i = 0; i < count; i++)
            {
                var hit = results[i];
                if (!hit.collider.isTrigger || !hit.collider.TryGetComponent(out IClickable clickable))
                {
                    continue;
                }
                
                clickable.Click();
            }
        }
        
        public bool TouchDisplay(out RaycastHit[] hits)
        {
            if (!Input.GetMouseButtonDown(0) || !gameObject.activeSelf)
            {
                hits = null;
                return false;
            }

            var ray = DataController.Instance.Cam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 20f, Color.red, 5f);
            var count = Physics.RaycastNonAlloc(ray, results, Mathf.Infinity, layerMask);
            hits = new RaycastHit[count];
            for (var index = 0; index < hits.Length; index++)
            {
                hits[index] = results[count];
            }

            return true;
        }

        public void Clear()
        {
            gameObject.SetActive(false);
            IsCustomUse = false;
            clickableList.Clear();
        }
    }
}

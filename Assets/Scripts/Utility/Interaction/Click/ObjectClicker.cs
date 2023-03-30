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

        private readonly List<IClickable> clickables = new List<IClickable>();

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
            if (clickables.Contains(clickable))
            {
                return;
            }
            clickables.Add(clickable);
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                Debug.Log("클리커 활성화" + gameObject.activeSelf);
            }
        }

        private void RemoveClick(IClickable clickable)
        {
            if (!clickables.Contains(clickable))
            {
                return;
            }
            clickables.Remove(clickable);
            if (clickables.Count == 0)
            {
                gameObject.SetActive(false);
                Debug.Log("클리커 비활성화" + gameObject.activeSelf);
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
            var hits = Physics.RaycastAll(ray, Mathf.Infinity, layerMask);
            foreach (var hit in hits)
            {
                if (hit.collider.TryGetComponent(out IClickable clickable))
                {
                    clickable.Click();
                    Debug.Log("됨");
                }
            }
        }
        
        public bool TouchDisplay(out RaycastHit[] hits)
        {
            if (!Input.GetMouseButtonDown(0) || !gameObject.activeSelf)
            {
                hits = null;
                return false;
            }

            Ray ray = DataController.Instance.Cam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 20f, Color.red, 5f);
            hits = Physics.RaycastAll(ray, Mathf.Infinity, layerMask);
            return true;
        }

        public void Reset()
        {
            gameObject.SetActive(false);
            IsCustomUse = false;
            clickables.Clear();
        }
    }
}

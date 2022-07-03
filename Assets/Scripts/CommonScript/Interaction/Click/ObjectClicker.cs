using System.Collections.Generic;
using UnityEngine;

namespace CommonScript
{
    public class ObjectClicker : MonoBehaviour
    {
        public static ObjectClicker instance;
        
        private int layerMask;

        public bool isCustomUse;

        private readonly List<IClickable> clickables = new List<IClickable>();

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            layerMask = 1 << LayerMask.NameToLayer("ClickObject") | 1 << LayerMask.NameToLayer("OnlyPlayerCheck");
            gameObject.SetActive(false);
        }

        void Update()
        {
            if(!isCustomUse)
                OnTouchDisplay();
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
            clickables.Add(clickable);
            if(!gameObject.activeSelf) gameObject.SetActive(true);
        }

        private void RemoveClick(IClickable clickable)
        {
            clickables.Remove(clickable);
            if(clickables.Count == 0) gameObject.SetActive(false);
        }
        
        public void OnTouchDisplay()
        {
            if (!Input.GetMouseButtonDown(0)) return;
        
            Ray ray = DataController.instance.cam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 20f, Color.red, 5f);
            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, layerMask);
            foreach (var hit in hits)
            {
                hit.collider.GetComponent<IClickable>().Click();
            }
        }
        
        public bool TouchDisplay(out RaycastHit[] hits)
        {
            if (!Input.GetMouseButtonDown(0) || !gameObject.activeSelf)
            {
                hits = null;
                return false;
            }

            Ray ray = DataController.instance.cam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 20f, Color.red, 5f);
            hits = Physics.RaycastAll(ray, Mathf.Infinity, layerMask);
            return true;
        }
    }
}

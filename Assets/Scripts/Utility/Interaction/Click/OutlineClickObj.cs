﻿using UnityEngine;

namespace Utility.Interaction.Click
{
    public class OutlineClickObj : MonoBehaviour, IClickable
    {
        public bool IsClickEnable
        {
            get => outline.enabled;
            set
            {
                outline.enabled = value;
                ActiveObjectClicker(value);
            }
        }

        public bool IsClicked { get; set; }
    
        [Header("#Outline setting")]
        public Color outlineColor = Color.red;

        public Vector3 offset;
        public float radius;
        public Outline.Mode mode = Outline.Mode.OutlineAll;
        public Outline outline;

        public void ActiveObjectClicker(bool isActive)
        {
            if (!ObjectClicker.Instance)
            {
                return;
            }
            ObjectClicker.Instance.UpdateClick(this, isActive);
        }

        public bool GetIsClicked()
        {
            if (IsClicked)
            {
                IsClicked = false;
                return true;
            }

            return false;
        }
        public void Click()
        {
            if (!IsClickEnable) return;
            IsClicked = true;
        }

        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
            if (outline == null)
            {
                outline = gameObject.AddComponent<Outline>();   
            }            
            outline.OutlineMode = mode;
            outline.OutlineColor = outlineColor;
            outline.OutlineWidth = 8f;
            outline.enabled = false;
        
            if (!gameObject.TryGetComponent(out SphereCollider sphereCol))
            {
                sphereCol = gameObject.AddComponent<SphereCollider>();   
            }
            sphereCol.isTrigger = true;
            radius /= transform.lossyScale.y;
            sphereCol.center = transform.InverseTransformPoint(transform.position + offset);
            sphereCol.radius = radius;
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = outlineColor - Color.black * 0.7f;
            Gizmos.DrawWireSphere(transform.position + offset, radius);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") || !enabled)
            {
                return;
            }
            IsClickEnable = true;
            Debug.Log("키기 - " + other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }
            IsClickEnable = false;
            Debug.Log("끄기");
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            IsClickEnable = false;
        }
        private void OnDestroy()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            IsClickEnable = false;
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.UI;
using Utility.Core;
using Utility.Preference;

namespace Episode.EP2.ThrowCanGame
{
    public class RingCreator : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Image ringImage;
        public GameObject rangeParent;
        
        [SerializeField] private int segments;
        [SerializeField] [Range(0, 1)] private float noteSpeed;

        [NonSerialized] public float Radius;
    
        private float originalRadius;
        
        private const float RingWidth = 10f;

        public void Setup()
        {
            JoystickController.Instance.InitializeJoyStick(false);
            DataController.Instance.CurrentMap.ui.gameObject.SetActive(true);

            var canvas = PlayUIController.Instance.Canvas;
    
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = DataController.Instance.Cam;
            canvas.planeDistance = 0.8f;
            
            rangeParent.SetActive(true);
            
            lineRenderer.positionCount = segments + 50;
            lineRenderer.useWorldSpace = false;

            lineRenderer.endWidth = RingWidth;
            lineRenderer.startWidth = RingWidth;
            lineRenderer.alignment = LineAlignment.TransformZ;
            originalRadius = ringImage.rectTransform.localScale.x / 3
                         * Mathf.Pow(ringImage.sprite.rect.width, 2)
                         / ringImage.sprite.pixelsPerUnit
                         / 2;
            
            Radius = originalRadius;

            var vec = transform.localPosition;
            vec.z = -.01f;
            transform.localPosition = vec;
            
            lineRenderer.enabled = true;
            
            CreatePoints();
        }

        public void Reset()
        {
            DataController.Instance.CurrentMap.ui.gameObject.SetActive(false);
            lineRenderer.enabled = false;
        }

        public void DisplayUpdate()
        {
            Radius -= originalRadius * noteSpeed * Time.deltaTime;
            CreatePoints();
        }

        public void CreatePoints()
        {
            for (var i = 0; i < lineRenderer.positionCount; i++)
            {
                lineRenderer.SetPosition(i,
                    Radius * new Vector3(Mathf.Cos(Mathf.Deg2Rad * 360 / segments * i),
                        Mathf.Sin(Mathf.Deg2Rad * 360 / segments * i)));
            }
        }
    }
}
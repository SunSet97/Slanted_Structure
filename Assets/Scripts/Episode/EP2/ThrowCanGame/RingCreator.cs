using System;
using UnityEngine;
using UnityEngine.UI;
using Utility.Core;
using Utility.UI.Preference;

namespace Episode.EP2.ThrowCanGame
{
    public class RingCreator : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Image ringImage;
        [SerializeField] private GameObject rangeParent;
        [SerializeField] private int segments;
        [SerializeField] private float ringWidth;

        [NonSerialized] public float Radius;
#pragma warning restore 0649
        
        private float originalRadius;

        public void Initialize()
        {
            JoystickController.Instance.SetJoyStickState(false);
            DataController.Instance.CurrentMap.ui.gameObject.SetActive(true);

            var canvas = PlayUIController.Instance.Canvas;
    
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = DataController.Instance.Cam;
            canvas.planeDistance = 0.8f;
            
            rangeParent.SetActive(true);
            
            lineRenderer.positionCount = segments + 50;
            lineRenderer.useWorldSpace = false;

            lineRenderer.endWidth = ringWidth;
            lineRenderer.startWidth = ringWidth;
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

        public void End()
        {
            PlayUIController.Instance.Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            DataController.Instance.CurrentMap.ui.gameObject.SetActive(false);
            
            lineRenderer.enabled = false;
            rangeParent.SetActive(false);
        }

        public void DisplayUpdate(float noteSpeed)
        {
            Radius -= originalRadius * noteSpeed * Time.deltaTime;
            CreatePoints();
        }

        private void CreatePoints()
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
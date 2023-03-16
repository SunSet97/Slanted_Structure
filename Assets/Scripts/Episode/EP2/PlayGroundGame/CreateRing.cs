using System;
using UnityEngine;
using UnityEngine.UI;

namespace Episode.EP2.PlayGroundGame
{
    public class CreateRing : MonoBehaviour
    {
        [SerializeField] private int segments;
        [SerializeField] [Range(0, 1)] private float noteSpeed;
        [SerializeField] private GameObject ring;

        [NonSerialized] public float Radius;
    
        private float nodeRadius;
        private LineRenderer line;

        public void Setup()
        {
            line = GetComponent<LineRenderer>();
            line.positionCount = segments + 50;
            line.useWorldSpace = false;

            line.endWidth = .1f * ring.GetComponent<Image>().sprite.rect.width;
            line.startWidth = .1f * ring.GetComponent<Image>().sprite.rect.width;
            line.alignment = LineAlignment.TransformZ;
            nodeRadius = ring.GetComponent<RectTransform>().localScale.x / 2
                         * Mathf.Pow(ring.GetComponent<Image>().sprite.rect.width, 2)
                         / ring.GetComponent<Image>().sprite.pixelsPerUnit;
            Radius = nodeRadius;

            var vec = transform.localPosition;
            vec.z = -.01f;
            transform.localPosition = vec;

            enabled = true;
            gameObject.SetActive(true);
        }

        public void Reset()
        {
            enabled = false;
            gameObject.SetActive(false);
        }

        public void DisplayUpdate()
        {
            Radius -= nodeRadius * noteSpeed * Time.deltaTime;
            CreatePoints();
        }

        public void CreatePoints()
        {
            for (var i = 0; i < line.positionCount; i++)
            {
                line.SetPosition(i,
                    Radius * new Vector3(Mathf.Cos(Mathf.Deg2Rad * 360 / segments * i),
                        Mathf.Sin(Mathf.Deg2Rad * 360 / segments * i)));
            }
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.UI;

public class CreateRing : MonoBehaviour
{
    [SerializeField] private int segments;
    [SerializeField] [Range(0, 1)] private float noteSpeed;

    [SerializeField] private GameObject ring;

    [NonSerialized] public float radius;
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
        radius = nodeRadius;

        Vector3 vec = transform.localPosition;
        vec.z = -.01f;
        transform.localPosition = vec;

        enabled = true;
        gameObject.SetActive(true);
    }

    public void Remove()
    {
        enabled = false;
        gameObject.SetActive(false);
    }

    public void DisplayUpdate()
    {
        radius -= nodeRadius * noteSpeed * Time.deltaTime;
        CreatePoints();
    }

    public void CreatePoints()
    {
        for (int i = 0; i < line.positionCount; i++)
        {
            line.SetPosition(i,
                radius * new Vector3(Mathf.Cos(Mathf.Deg2Rad * 360 / segments * i),
                    Mathf.Sin(Mathf.Deg2Rad * 360 / segments * i)));
        }
    }
}
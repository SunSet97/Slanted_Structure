﻿using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Unity Editor의 Hierarchy창에 select한 경우 조이스틱 홀더가 0,0으로 리셋되는 경우가 있음
/// </summary>
public class DynamicJoystick : Joystick
{
    public float MoveThreshold
    {
        get { return moveThreshold; }
        set { moveThreshold = Mathf.Abs(value); }
    }

    [SerializeField] private float moveThreshold = 1;

    protected override void Start()
    {
        MoveThreshold = moveThreshold;
        base.Start();
        background.gameObject.SetActive(false);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        // Debug.Log(eventData);
        // Debug.Log(ScreenPointToAnchoredPosition(eventData.position));
        // Debug.Log(eventData.position);
        // background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        background.position = eventData.position;
        background.gameObject.SetActive(true);
        base.OnBeginDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        background.gameObject.SetActive(false);
        base.OnEndDrag(eventData);
    }

    protected override void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
    {
        var rectTransform = GetComponent<RectTransform>();
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;

        if (magnitude > moveThreshold)
        {
            Vector2 difference = normalised * (magnitude - moveThreshold) * radius;
            background.anchoredPosition += difference;

            Vector2 position = background.anchoredPosition;
            if (width < background.anchoredPosition.x)
            {
                position.x = width;
            }
            else if (0 > background.anchoredPosition.x)
            {
                position.x = 0;
            }

            if (height <= background.anchoredPosition.y)
            {
                position.y = height;
            }
            else if (0 > background.anchoredPosition.y)
            {
                position.y = 0;
            }

            background.anchoredPosition = position;
        }

        base.HandleInput(magnitude, normalised, radius, cam);
    }
}
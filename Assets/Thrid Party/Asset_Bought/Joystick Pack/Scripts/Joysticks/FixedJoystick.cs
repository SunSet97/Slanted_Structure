using UnityEngine;
using UnityEngine.EventSystems;

public class FixedJoystick : Joystick
{
    protected override void Start()
    {
        base.Start();
        background.gameObject.SetActive(false);
    }
    
    public override void OnBeginDrag(PointerEventData eventData)
    {
        background.position = eventData.position;
        background.gameObject.SetActive(true);
        base.OnBeginDrag(eventData);
    }
    
    public override void OnEndDrag(PointerEventData eventData)
    {
        background.gameObject.SetActive(false);
        base.OnEndDrag(eventData);
    }
    
    public override void OnDrag(PointerEventData eventData)
    {
        cam = null;
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            cam = canvas.worldCamera;

        Vector2 position = RectTransformUtility.WorldToScreenPoint(cam, background.position);
        Vector2 backgroundRadius = background.sizeDelta / 2;
        Vector2 handleRadius = (handle.sizeDelta / 2) + Vector2.one * 10;
        input = (eventData.position - position) / ((backgroundRadius - handleRadius) * canvas.scaleFactor);
        FormatInput();
        HandleInput(input.magnitude, input.normalized, (backgroundRadius - handleRadius), cam);
        handle.anchoredPosition = input * (backgroundRadius - handleRadius) * handleRange;
    }
}
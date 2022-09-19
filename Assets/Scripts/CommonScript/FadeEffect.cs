using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeEffect : MonoBehaviour
{
    public static FadeEffect instance { get; private set; }

    public CanvasGroup canvasGroup;
    public GraphicRaycaster graphicRaycaster;
    public bool isFadeOver;

    private void Awake()
    {
        instance = this;
    }

    public IEnumerator FadeIn()
    {
        DataController.instance.StopSaveLoadJoyStick(true);
        float time = 4;
        
        isFadeOver = false;
        graphicRaycaster.enabled = true;
        float t = 1;
        while (t > 0)
        {
            t -= Time.deltaTime / time;
            canvasGroup.alpha = t;
            yield return null;
        }

        canvasGroup.alpha = 0;
        graphicRaycaster.enabled = false;
        isFadeOver = true;
        DataController.instance.StopSaveLoadJoyStick(false);
    }
    
    public IEnumerator FadeOut()
    {
        DataController.instance.StopSaveLoadJoyStick(true);
        
        float time = 4;
        isFadeOver = false;
        var value = LayerMask.GetMask("Default");
        graphicRaycaster.enabled = true;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / time;
            canvasGroup.alpha = t;
            yield return null;
        }

        canvasGroup.alpha = 1;
        graphicRaycaster.enabled = false;
        isFadeOver = true;
        DataController.instance.StopSaveLoadJoyStick(false);
    }
}

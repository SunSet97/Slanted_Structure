﻿using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FadeEffect : MonoBehaviour
{
    public static FadeEffect instance { get; private set; }

    public Image fadePanel;
    public GraphicRaycaster graphicRaycaster;
    public bool isFadeOver;

    public bool isFadeOut;

    internal UnityEvent onFadeOver;
    private void Awake()
    {
        instance = this;
        onFadeOver = new UnityEvent();
    }

    public void FadeIn()
    {
        isFadeOut = false;
        isFadeOver = false;
        StartCoroutine(FadeInCoroutine());
    }
    
    private IEnumerator FadeInCoroutine()
    {
        JoystickController.instance.StopSaveLoadJoyStick(true);
        graphicRaycaster.enabled = false;
        fadePanel.gameObject.SetActive(true);

        float time = 4;
        
        float t = 1;
        Color color;
        while (t > 0)
        {
            t -= Time.deltaTime / time;
            color = fadePanel.color;
            color.a = t;
            fadePanel.color = color;
            yield return null;
        }

        color = fadePanel.color;
        color.a = 0;
        fadePanel.color = color;
        graphicRaycaster.enabled = true;
        isFadeOver = true;
        JoystickController.instance.StopSaveLoadJoyStick(false);
        fadePanel.gameObject.SetActive(false);

        onFadeOver?.Invoke();
        onFadeOver?.RemoveAllListeners();
    }

    public void FadeOut()
    {
        isFadeOver = false;
        isFadeOut = true;
        StartCoroutine(FadeOutCoroutine());
    }
    private IEnumerator FadeOutCoroutine()
    {
        JoystickController.instance.StopSaveLoadJoyStick(true);
        graphicRaycaster.enabled = false;
        fadePanel.gameObject.SetActive(true);
        
        float time = 4;
        var value = LayerMask.GetMask("Default");
        float t = 0;
        Color color;
        while (t < 1)
        {
            t += Time.deltaTime / time;
            color = fadePanel.color;
            color.a = t;
            fadePanel.color = color;
            yield return null;
        }

        color = fadePanel.color;
        color.a = 1;
        fadePanel.color = color;
        graphicRaycaster.enabled = true;
        isFadeOver = true;
        JoystickController.instance.StopSaveLoadJoyStick(false);
        
        onFadeOver?.Invoke();
        onFadeOver?.RemoveAllListeners();
    }
}

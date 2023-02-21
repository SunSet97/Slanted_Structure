﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utility.Core;

public class FadeEffect : MonoBehaviour
{
    private static FadeEffect _instance;
    public static FadeEffect Instance => _instance;

    public Image fadePanel;
    public GraphicRaycaster graphicRaycaster;
    [NonSerialized] public bool IsFadeOver;
    [NonSerialized] public bool IsAlreadyFadeOut;
    internal UnityEvent OnFadeOver;

    private void Awake()
    {
        if (_instance)
        {
            Debug.Log("FadeEffect 파괴");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("FadeEffect 생성");
            _instance = this;
            OnFadeOver = new UnityEvent();
        }
    }

    public void FadeIn(float fadeInSec = 2f)
    {
        if (!IsAlreadyFadeOut)
        {
            Debug.LogWarning("경고경고, FadeOut이 아닌 상태에서 FadeIn 실행");
            
            // IsFadeOver = true;
            // OnFadeOver?.Invoke();
            // return;
        }
        
        IsFadeOver = false;
        Debug.Log("Fade In");
        StopAllCoroutines();
        StartCoroutine(FadeInCoroutine(fadeInSec));
    }

    private IEnumerator FadeInCoroutine(float fadeInSec)
    {
        JoystickController.instance.StopSaveLoadJoyStick(true);
        graphicRaycaster.enabled = false;
        fadePanel.gameObject.SetActive(true);

        float t = 1;
        Color color;
        while (t > 0)
        {
            t -= Time.deltaTime / fadeInSec;
            color = fadePanel.color;
            color.a = t;
            fadePanel.color = color;
            yield return null;
        }

        color = fadePanel.color;
        color.a = 0;
        fadePanel.color = color;
        
        JoystickController.instance.StopSaveLoadJoyStick(false);
        
        graphicRaycaster.enabled = true;
        IsAlreadyFadeOut = false;
        IsFadeOver = true;
        
        fadePanel.gameObject.SetActive(false);

        Debug.Log("Fade In 종료");
        OnFadeOver?.Invoke();
    }

    public void FadeOut(float fadeOutSec = 2f)
    {
        if (IsAlreadyFadeOut)
        {
            Debug.LogWarning("이미 FadeOut임, 오류");
            IsFadeOver = true;
            OnFadeOver?.Invoke();
            return;
        }
        
        IsFadeOver = false;
        IsAlreadyFadeOut = true;
        Debug.Log("Fade Out");
        StopAllCoroutines();
        StartCoroutine(FadeOutCoroutine(fadeOutSec));
    }

    private IEnumerator FadeOutCoroutine(float fadeOutSec)
    {
        JoystickController.instance.StopSaveLoadJoyStick(true);
        graphicRaycaster.enabled = false;
        fadePanel.gameObject.SetActive(true);
        
        float t = 0f;
        Color color;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeOutSec;
            color = fadePanel.color;
            color.a = t;
            fadePanel.color = color;
            yield return null;
        }
        
        color = fadePanel.color;
        color.a = 1;
        fadePanel.color = color;
        
        JoystickController.instance.StopSaveLoadJoyStick(false);
        
        graphicRaycaster.enabled = true;

        IsFadeOver = true;
        
        Debug.Log("Fade Out 종료");
        
        OnFadeOver?.Invoke();
    }
}

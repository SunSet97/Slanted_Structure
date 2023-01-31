using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utility.System;

public class FadeEffect : MonoBehaviour
{
    private static FadeEffect _instance;

    public static FadeEffect instance => _instance;

    public Image fadePanel;
    public GraphicRaycaster graphicRaycaster;
    public bool isFadeOver;

    public bool isFadeOut;

    internal UnityEvent onFadeOver;

    public bool isFaded;
    private void Awake()
    {
        if (_instance)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            Destroy(_instance);
        }
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
        isFaded = true;

        onFadeOver?.Invoke();
        onFadeOver?.RemoveAllListeners();
        isFaded = false;
    }

    public void FadeOut()
    {
        if (!isFadeOut)
        {
            isFadeOver = false;
            isFadeOut = true;
            StartCoroutine(FadeOutCoroutine());
        }
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
        
        isFaded = true;
        onFadeOver?.Invoke();
        onFadeOver?.RemoveAllListeners();
        isFaded = false;
    }
}

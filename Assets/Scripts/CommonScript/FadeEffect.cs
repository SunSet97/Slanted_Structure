using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utility.Core;

public class FadeEffect : MonoBehaviour
{
    public static FadeEffect Instance { get; private set; }

    public Image fadePanel;
    public GraphicRaycaster graphicRaycaster;
    [NonSerialized] public bool IsFadeOver;
    [NonSerialized] public bool IsFaded;
    [NonSerialized] public bool IsFadeOut;
    internal UnityEvent OnFadeOver;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            Destroy(Instance);
        }
        OnFadeOver = new UnityEvent();
    }

    public void FadeIn(float fadeInSec = 2f)
    {
        IsFadeOut = false;
        IsFadeOver = false;
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
        graphicRaycaster.enabled = true;
        IsFadeOver = true;
        JoystickController.instance.StopSaveLoadJoyStick(false);
        fadePanel.gameObject.SetActive(false);
        IsFaded = true;

        OnFadeOver?.Invoke();
        OnFadeOver?.RemoveAllListeners();
        IsFaded = false;
    }

    public void FadeOut(float fadeOutSec = 2f)
    {
        if (!IsFadeOut)
        {
            IsFadeOver = false;
            IsFadeOut = true;
            StartCoroutine(FadeOutCoroutine(fadeOutSec));
        }
    }

    private IEnumerator FadeOutCoroutine(float fadeOutSec)
    {
        JoystickController.instance.StopSaveLoadJoyStick(true);
        graphicRaycaster.enabled = false;
        fadePanel.gameObject.SetActive(true);
        
        var value = LayerMask.GetMask("Default");
        float t = 0;
        Color color;
        while (t < 1)
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
        graphicRaycaster.enabled = true;
        IsFadeOver = true;
        JoystickController.instance.StopSaveLoadJoyStick(false);
        
        IsFaded = true;
        OnFadeOver?.Invoke();
        OnFadeOver?.RemoveAllListeners();
        IsFaded = false;
    }
}

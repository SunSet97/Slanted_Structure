using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeEffect : MonoBehaviour
{
    public static FadeEffect instance { get; private set; }
    public Image fadeImage;

    public bool isFade;

    private void Awake()
    {
        instance = this;
    }

    public IEnumerator FadeIn()
    {
        isFade = false;
        fadeImage.raycastTarget = true;
        float t = 1;
        while (t > 0)
        {
            t -= Time.deltaTime;
            var c = fadeImage.color;
            c.a = t;
            fadeImage.color = c;
            yield return null;
        }
        fadeImage.raycastTarget = false;
        isFade = true;
    }
    
    public IEnumerator FadeOut()
    {
        isFade = false;
        fadeImage.raycastTarget = true;
        float t = 0;
        while (t > 1)
        {
            t += Time.deltaTime;
            var c = fadeImage.color;
            c.a = t;
            fadeImage.color = c;
            yield return null;
        }

        fadeImage.raycastTarget = false;
        isFade = true;
    }
}

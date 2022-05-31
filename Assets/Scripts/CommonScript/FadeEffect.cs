using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeEffect : MonoBehaviour
{
    public static FadeEffect instance { get; private set; }
    public Image fadeImage;

    public bool isFadeOver;

    private void Awake()
    {
        instance = this;
    }

    public IEnumerator FadeIn()
    {
        DataController.instance.StopSaveLoadJoyStick(true);
        SetImage(true);
        float time = 4;
        
        isFadeOver = false;
        fadeImage.raycastTarget = true;
        float t = 1;
        while (t > 0)
        {
            t -= Time.deltaTime / time;
            var c = fadeImage.color;
            c.a = t;
            fadeImage.color = c;
            yield return null;
        }
        fadeImage.raycastTarget = false;
        isFadeOver = true;
        DataController.instance.StopSaveLoadJoyStick(false);
    }
    
    public IEnumerator FadeOut()
    {
        DataController.instance.StopSaveLoadJoyStick(true);
        SetImage(true);
        float time = 4;
        isFadeOver = false;
        fadeImage.raycastTarget = true;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / time;
            Debug.Log(t);
            var c = fadeImage.color;
            c.a = t;
            fadeImage.color = c;
            yield return null;
        }

        fadeImage.raycastTarget = false;
        isFadeOver = true;
        DataController.instance.StopSaveLoadJoyStick(false);
    }

    public void SetImage(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
}

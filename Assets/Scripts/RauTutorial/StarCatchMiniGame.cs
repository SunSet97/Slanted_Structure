using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class StarCatchMiniGame : MiniGameClass
{
    public Slider slider;
    public RectTransform fillArea;
    [Range(0.0f, 1.0f)] public float startRange;
    [Range(0.0f, 1.0f)] public float endRange;
    [Range(0.0f, 1.0f)] public float increaseValue;
    
    private bool isPlay = true;
    private bool invert = true;
    private float value = 0;

    public override void on()
    {
        this.gameObject.SetActive(true);
        settings();
        starCoroutine();
    }

    public override void off()
    {
        this.gameObject.SetActive(false);
        value = 0;
    }

    public void settings()
    {
        settings(startRange,endRange);
    }
    public void settings(float minX, float maxX)
    {
        fillArea.anchorMin = new Vector2(minX,fillArea.anchorMin.y);
        fillArea.anchorMax = new Vector2(maxX,fillArea.anchorMax.y);
    }
    
    public void stop()
    {
        isPlay = checkResult();

        if (isPlay)
        {
            Debug.Log("Success");
            isPlay = false;
            callBack();
        }
        else
        {
            Debug.Log("Fail");
            isPlay = true;
        }
    }

    public bool checkResult()
    {
        return (startRange <= value && value <= endRange);
    }
    
    public void starCoroutine()
    {
        StartCoroutine(catchCoroutine());
    }
    
    IEnumerator catchCoroutine()
    {
        WaitForSeconds time = new WaitForSeconds(1 * Time.deltaTime);

        while (isPlay)
        {
            value += (invert ? 1 : -1) * increaseValue;
            
            checkValueRange();
            
            slider.value = value;
            
            yield return time;
        }
    }

    public void checkValueRange()
    {
        if (value <= 0)
        {
            value = 0;
            invert = true;
        }

        if (value >= 1)
        {
            value = 1;
            invert = false;
        }
    }
}

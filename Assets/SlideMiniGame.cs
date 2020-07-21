using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SlideMiniGame : MiniGameClass
{
    public GameObject grass;
    public Slider slider;
    public float sliderFadeValue;
    public float nextSlideTime;

    private bool isReady = false;
    private int slideCount;
    private GameObject actor;
    private Image[] imageChild;
    private List<Transform> tsChild;
    private Color alphaColor = new Color(0,0,0,1);

    public void Start()
    {
        imageChild = slider.GetComponentsInChildren<Image>();
        actor = DataController.instance_DataController.currentChar.gameObject;
        tsChild = grass.GetComponentsInChildren<Transform>().ToList();
    }

    public void onChange()
    {
        if (checkValue())
        {
            var go = getNearGrass();

            StartCoroutine(slideObject(go, 0.1f, Vector3.forward * (go.transform.localPosition.z > 0 ? 1 : -1)));
            
            slider.interactable = false;
            offSlider();
        }
    }

    public override void on()
    {
        this.gameObject.SetActive(true);
        DataController.instance_DataController.joyStick.input = Vector2.zero;
        fadeSlider(-1);
        onSlider();
    }

    public override void off()
    {
        this.gameObject.SetActive(false);
        offSlider();
    }

    public bool checkValue()
    {
        return (slider.value >= 1);
    }

    public void changeSlider(float rotZ)
    {
        slider.gameObject.transform.eulerAngles = Vector3.forward * rotZ;
    }

    public void initSlider()
    {
        slider.value = 0.001f;
    }

    IEnumerator fadeCoroutine(float degreeValue, Action _action)
    {
        float absValue = Mathf.Abs(degreeValue);
        float value = 0;
        WaitForSeconds seconds = new WaitForSeconds(absValue);

        while (true)
        {
            fadeSlider(degreeValue);
            value += absValue;
            yield return seconds;

            if (value >= 1)
                break;
        }

        _action();
    }

    IEnumerator delayCoroutine(float _time,Action _action)
    {
        WaitForSeconds seconds = new WaitForSeconds(_time);

        yield return seconds;

        _action();
    }

    IEnumerator slideObject(GameObject go, float _time, Vector3 _direction)
    {
        WaitForSeconds seconds = new WaitForSeconds(_time * Time.deltaTime);
        float time = 0;
        while (true)
        {
            go.transform.position += _direction * _time;
            time += _time;
            yield return seconds;

            if (time >= 1)
                break;
        }
    }

    public void fadeSlider(float degreeValue)
    {
        foreach (var graphic in imageChild)
        {
            graphic.color += alphaColor * degreeValue;
        }
    }

    public void onSlider()
    {
        var go = getNearGrass(false);
        var value = go.transform.localPosition.z > 0 ? 1 : 0;
        changeSlider(value * 180);
        StartCoroutine(fadeCoroutine(sliderFadeValue, () =>
        {
            isReady = true;
            slider.interactable = true;
        }));
    }

    public void offSlider()
    {
        StartCoroutine(fadeCoroutine(-sliderFadeValue, () =>
        {
            initSlider();
            StartCoroutine(delayCoroutine(nextSlideTime, onSlider));
        }));
    }

    public GameObject getNearGrass(bool remoteIt = true)
    {
        float distance = float.MaxValue;
        int index = int.MaxValue;

        for (int i = 0; i < tsChild.Count; i++)
        {
            var d = Vector3.Distance(actor.transform.position, tsChild[i].position);
            if (d < distance)
            {
                distance = d;
                index = i;
            }
        }

        var returnValue = tsChild[index].gameObject;
        
        if(remoteIt)
            tsChild.RemoveAt(index);
        
        return returnValue;
    }
}

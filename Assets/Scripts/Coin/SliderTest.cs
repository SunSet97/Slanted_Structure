using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderTest : MonoBehaviour
{
    public Slider slider;
    float maxSlider = 100;
    bool isSliderMax = false;
    float Power = 0;
    Rigidbody rigid;
    float value = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        slider.maxValue = 100;
        slider.minValue = 0;
        slider.wholeNumbers = true;
        slider.value = 50;
        rigid = GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    void Update()
    {
        //누르고 있는 동안 게이지 왔다갔다
        if (Input.GetMouseButtonDown(0))
        {
            if (!isSliderMax && slider.value < maxSlider)
            {
                slider.value += Time.deltaTime * value;
                if (slider.value >= slider.maxValue) 
                    isSliderMax = true;
            }
            else
            {
                slider.value -= Time.deltaTime * value;
                if (slider.value <= 0) 
                    isSliderMax = true;
            }
        }

        //Power = rigid.AddForce(Vector3.up * slider.value, ForceMode.Impulse);

    }
}

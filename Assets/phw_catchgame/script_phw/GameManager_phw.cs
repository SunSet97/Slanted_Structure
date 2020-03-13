using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager_phw : MonoBehaviour
{
    public Bicycle_phw bicycle;
    public PickPocket_phw PP;
    public Rau_phw rau;
    public float timer;
    public Text textTimer;
    public Text textItem;
    public float completeDistance;
    bool gameover;
    public CanvasGroup failPanel;
    public CanvasGroup completePanel;

    void Start() {
        gameover = false;
    }
    

    void Update()
    {
        timer -= Time.deltaTime;
        textTimer.text = "남은 시간: " + Mathf.Round(timer);
        textItem.text = "아이템 개수: " + rau.itemCount;

        // 못잡음
        if (Mathf.Round(timer) == 0)
        {
            failPanel.alpha = 1;
            Time.timeScale = 0;
        }
        

        //잡기 성공
        if ((int)PP.transform.position.z - (int)rau.transform.position.z == completeDistance) {
            completePanel.alpha = 1;
            Time.timeScale = 0;
        }


        // 바이클 작동시키기
        if (bicycle.bicycleSettingComplete == false && Mathf.Round(timer) % bicycle.appearTime == 0) {
            Debug.Log("겜메니져. 바이클 active하기");
            bicycle.gameObject.SetActive(true);
            
        }
        
       

        }


    }

    

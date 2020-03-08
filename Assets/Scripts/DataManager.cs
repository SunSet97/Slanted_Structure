﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using UnityEngine.SceneManagement;


public class DataManager : MonoBehaviour
{
    [Header("싱글톤")]

    public bool[] isExistdata = new bool[3];
    CanvasControl canvasCtrl;
    GameObject player;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        ExistsData();
        
    }

    static DataManager instance;
    public static DataManager Instance
    {
        get
        {
            var obj = FindObjectOfType<DataManager>();

            if (obj != null)

            {

                instance = obj;

            }

            return instance;
        }
    }

    // 데이터 저장 버튼을 누르면 불리는 함수
    public void SelectData(int fileNum)
    {
        if (player == null)
        {
            player = GameObject.Find("Player");
        }

        if (canvasCtrl == null)
        {
            canvasCtrl = GameObject.Find("Canvas").GetComponent<CanvasControl>();
        }

        if (DataController.Instance.charData.pencilCnt > 0)
        {
            // 기존 데이터 없을 경우 버튼 텍스트 업데이트
            if (isExistdata[fileNum] == false)
            {
                isExistdata[fileNum] = true;
                canvasCtrl.saveText[fileNum].text = "FULL DATA";
            }

            // 데이터 저장 시 연필 개수, 캐릭터 위치, 현재 씬 등 업데이트 (점점 추가할 예정)
            DataController.Instance.charData.pencilCnt -= 1;
            DataController.Instance.charData.endPosition = player.transform.position;
            DataController.Instance.charData.currentScene = SceneManager.GetActiveScene().name;

            DataController.Instance.SaveCharData("SaveData" + fileNum);

        }
    }

    // 데이터 파일이 존재하는 지 확인하는 함수
    public void ExistsData()
    {
        for (int i = 0; i < 3; i++)
        {
            string filePath = Application.persistentDataPath + "/" + "SaveData" + i;

            if (File.Exists(filePath))
            {
                isExistdata[i] = true;
            }
            else
            {
                isExistdata[i] = false;
            }
        }
    }

}

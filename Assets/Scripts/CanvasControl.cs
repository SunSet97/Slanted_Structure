using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class CanvasControl : MonoBehaviour
{
    public Text[] saveText = new Text[3];
    public GameObject savePanel;
    bool isExistFile;

    public void OpenPanel(string panelName)
    {
        if (panelName == "SavePanel" || panelName == "SelectData")
        {
            savePanel.SetActive(true);
            // 세이브 데이터 패널이 열릴 때마다 각 세이브 파일이 존재하는 지 확인 
            DataManager.instance_DataManager.ExistsData();

            for (int i = 0; i < 3; i++)
            {

                if (DataManager.instance_DataManager.isExistdata[i])
                {
                    // 해당 칸에 데이턱 존재하면 버튼 텍스트 업데이트
                    saveText[i].text = "FULL DATA";
     
                }
                else
                {
                    // 데이터 없을 때
                    saveText[i].text = "NO DATA";
                } 
            }
        }
    }

    public void ClosePanel(string panelName)
    {
        if (panelName == "SavePanel")
        {
            savePanel.SetActive(false);
        }

    }
}

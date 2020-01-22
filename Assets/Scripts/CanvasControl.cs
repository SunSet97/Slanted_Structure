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


            for (int i = 0; i < 3; i++)
            {

                if (DataManager.Instance.isExistdata[i])
                {
                    saveText[i].text = "FUll DATA";
     
                }
                else
                {
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

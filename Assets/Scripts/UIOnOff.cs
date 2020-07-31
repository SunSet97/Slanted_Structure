
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIOnOff : MonoBehaviour
{
    public GameObject map;
    public GameObject ui;
    
    void Update()
    {
        ui.SetActive(map.activeSelf);
    }
}
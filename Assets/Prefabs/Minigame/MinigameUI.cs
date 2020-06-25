
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameUI : MonoBehaviour
{
    public GameObject map;
    public GameObject ui;
    
    void Update()
    {
        ui.SetActive(map.activeSelf);
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public struct CameraSetting
{
    public bool isLookAt;
    public GameObject target;
    public Vector3 _relativePosition;
    public Vector3 rotate;
}

public class CameraController : MonoBehaviour
{
    private Dictionary<string, CameraSetting> _camSettings;

    private static CameraController _instance;
    private const string camStr = "CamSetting";

    public static CameraController Instance
    {
        get
        {
            if(_instance == null)
                _instance = new CameraController();
            return _instance;
        }
    }

    private CameraController()
    {
        if (_instance == null)
            _instance = this;
        
        _camSettings = new Dictionary<string, CameraSetting>();
    }

    public void changeCameraSetting(string str)
    {
        var setting = getCamSetting(str);

        if (setting == null)
            return;
        
        
    }

    public CameraSetting? getCamSetting(string str)
    {
        if (_camSettings.ContainsKey(str))
            return _camSettings[str];

        return null;
    }
    
    public string addCamSetting(CameraSetting setting,string str = null)
    {
        if(str == null)
            str = camStr + (_camSettings.Count + 1);
        
        _camSettings.Add(str,setting);

        return str;
    }
}

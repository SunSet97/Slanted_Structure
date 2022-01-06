using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayable
{
    bool IsPlay { get; set; }
}
[Serializable]
public struct PlayableList
{
    public PlayableObj[] playableObjs;
}

[Serializable]
public struct PlayableObj
{
    public GameObject gameObject;
    public bool isPlay;
}

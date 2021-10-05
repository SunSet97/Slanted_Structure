using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Playable
{
    bool isPlay { get; set; }
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

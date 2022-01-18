using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IPlayable
{
    bool IsPlay { get; set; }

    void Play();
    void EndPlay();
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

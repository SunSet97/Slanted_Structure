using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Playable
{
    bool isPlay { get; set; }
}
[SerializeField]
public struct PlayableList
{
    public PlayableObj[] playableObjs;
}

[SerializeField]
public struct PlayableObj
{
    public GameObject gameObject;
    public bool isPlay;
}

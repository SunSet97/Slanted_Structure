using System;
using System.Collections.Generic;
using UnityEngine;
public interface Movable
{
    bool IsMove { get; set; }
}
[Serializable]
public struct MovableList
{
    public MovableObj[] movables;
}
[Serializable]
public class MovableObj
{
    public GameObject gameObject;
    public bool isMove;
}
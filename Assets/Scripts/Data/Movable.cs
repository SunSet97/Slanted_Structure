using System;
using System.Collections.Generic;
using UnityEngine;
public interface Movable
{
    bool IsMove { get; set; }
}
[Serializable]
public class MovableObj
{
    public List<GameObject> gameObjects;
}
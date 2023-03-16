using System;
using UnityEngine;

namespace Move
{
    public interface IMovable
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
}


public interface IClickable
{
    bool IsClickEnable { get; set; }

    bool IsClicked { get; set; }

    void ActiveObjectClicker(bool isActive);
    bool GetIsClicked();
    void Click();
}
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


namespace Play
{
    public interface IGamePlayable
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
}


public interface IClickable
{
    bool IsClickEnable { get; set; }

    bool IsClicked { get; set; }

    void ActiveObjectClicker(bool isActive);
    bool GetIsClicked();
    void Click();
}
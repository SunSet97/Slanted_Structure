using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Utility.Game
{
    public abstract class MiniGame : MonoBehaviour
    {
        public Action<bool> OnEndPlay;

        [NonSerialized] public bool IsPlay;

        public virtual void Play()
        {
            Debug.Log("PlayGame");
            IsPlay = true;
        }

        public virtual void EndPlay(bool isSuccess)
        {
            Debug.Log("EndGame");
            IsPlay = false;
            OnEndPlay?.Invoke(isSuccess);
        }
    }

    [Serializable]
    public struct PlayableList
    {
        public PlayableObj[] playableObjs;
    }

    [Serializable]
    public struct PlayableObj
    {
        [FormerlySerializedAs("game")] public MiniGame miniGame;
        public bool isPlay;
    }
}
using System;
using UnityEngine;
using UnityEngine.Serialization;
using Utility.Interaction;

namespace Data
{
    [Serializable]
    public struct CamInfo {
        public Vector3 camDis;
        public Vector3 camRot;
    }

    [Serializable]
    public class WaitInteractionData
    {
        public WaitInteraction[] waitInteractions;
    }
    
    [Serializable]
    public struct WaitInteraction
    {
        public InteractionObject waitInteraction;
        public int interactionIndex;
    }

    namespace GamePlay
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
}
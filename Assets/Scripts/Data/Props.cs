using System;
using UnityEngine;
using UnityEngine.Serialization;
using Utility.Interaction;

namespace Data
{
    [Serializable]
    public class CamInfo {
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
            public Action OnEndPlay;
        
            [NonSerialized] public bool IsPlay;

            public virtual void Play()
            {
                IsPlay = true;
            }

            public virtual void EndPlay()
            {
                IsPlay = false;
                OnEndPlay?.Invoke();
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
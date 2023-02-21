using System;
using System.Collections.Generic;
using CommonScript;
using Move;
using Play;
using UnityEngine;
using UnityEngine.Events;
using Utility.Core;
using Utility.Property;

namespace Utility.Interaction
{
    [Serializable]
    public class InteractionEvents
    {
        [SerializeField] public List<InteractionEvent> interactionEvents;

        public void AddInteraction(UnityAction unityAction)
        {
            interactionEvents.Add(new InteractionEvent
            {
                eventType = InteractionEvent.EventType.Custom,
                UnityAction = unityAction
            });
        }
    }

    [Serializable]
    public class InteractionEvent
    {
        public enum EventType
        {
            None,
            Clear,
            Active,
            Move,
            Play,
            Interactable,
            Interaction,
            // FadeOut,
            // PlayAudio,
            Custom
        }

        public EventType eventType;

        [Serializable]
        public struct Act
        {
            public GameObject activeObject;
            public bool activeSelf;
        }

        [Serializable]
        public struct Active
        {
            public Act[] actives;
        }

        [Serializable]
        public struct Interact
        {
            public InteractionObject interactObj;
            public bool interactable;
            public int index;
        }

        [Serializable]
        public struct InteractList
        {
            public Interact[] interacts;
        }
        
        [Serializable]
        public struct Interaction
        {
            public InteractionObject interactObj;
            public int index;
            public int waitSeconds;
            public bool useFadeOut;
        }
        
        [Serializable]
        public struct InteractionList
        {
            public Interaction[] interactions;
        }
        
        [Serializable]
        public struct Audio
        {
            public AudioClip audioClip;
            public bool isSfx;
            public bool isBgm;
        }

        [ConditionalHideInInspector("eventType", EventType.Clear)]
        public CheckMapClear clearBox;

        [ConditionalHideInInspector("eventType", EventType.Active)]
        public Active activeObjs;

        [ConditionalHideInInspector("eventType", EventType.Move)]
        public MovableList movables;

        [ConditionalHideInInspector("eventType", EventType.Play)]
        public PlayableList playableList;

        [ConditionalHideInInspector("eventType", EventType.Interactable)]
        public InteractList interactObjs;
        
        [ConditionalHideInInspector("eventType", EventType.Interaction)]
        public InteractionList interactionObjs;
        
        // [ConditionalHideInInspector("eventType", EventType.FadeOut)]
        // public float fadeSec;
        
        // [ConditionalHideInInspector("eventType", EventType.PlayAudio)]
        // public Audio audio;

        [NonSerialized] public UnityAction UnityAction;

        public void Action()
        {
            switch (eventType)
            {
                case EventType.Clear:
                    ClearEvent();
                    break;
                case EventType.Active:
                    ActiveEvent();
                    break;
                case EventType.Move:
                    MoveEvent();
                    break;
                case EventType.Play:
                    PlayEvent();
                    break;
                case EventType.Interactable:
                    InteractableEvent();
                    break;
                case EventType.Interaction:
                    InteractEvent();
                    break;
                // case EventType.FadeOut:
                //     FadeOut();
                //     break;
                // case EventType.PlayAudio:
                //     PlayAudio();
                //     break;
                case EventType.Custom:
                    UnityAction?.Invoke();
                    break;
            }
        }



        private void ClearEvent()
        {
            Debug.Log($"Clear Event - {clearBox.gameObject}");
            clearBox.Clear();
        }

        private void ActiveEvent()
        {
            foreach (var t in activeObjs.actives)
            {
                Debug.Log($"ACTIVE Event - {t.activeObject}: {t.activeSelf}");
                t.activeObject.SetActive(t.activeSelf);
            }
        }

        private void MoveEvent()
        {
            foreach (MovableObj t in movables.movables)
            {
                Debug.Log($"Move Event - {t.gameObject}: {t.isMove}");
                t.gameObject.GetComponent<IMovable>().IsMove = t.isMove;
            }
        }

        private void PlayEvent()
        {
            foreach (var t in playableList.playableObjs)
            {
                Debug.Log($"Play Event - {t.gameObject}: {t.isPlay} ");
                t.gameObject.GetComponent<IGamePlayable>().Play();
            }
        }

        private void InteractableEvent()
        {
            foreach (var t in interactObjs.interacts)
            {
                var interaction = t.interactObj.GetInteraction(t.index);
                interaction.serializedInteractionData.isInteractable = t.interactable;
            }
        }
        
        private void InteractEvent()
        {
            foreach (var t in interactionObjs.interactions)
            {
                t.interactObj.InteractIndex = t.index;
                t.interactObj.GetInteraction().serializedInteractionData.isInteractable = true;
                t.interactObj.Invoke(nameof(InteractionObject.StartInteraction), t.waitSeconds);
            }
        }
        
        // private void PlayAudio()
        // {
        //     if (audio.isSfx)
        //     {
        //         AudioController.instance.PlayOneShot(audio.audioClip);
        //     }
        //     else if (audio.isBgm)
        //     {
        //         // AudioController.instance.PlayBgm(audio.audioClip);
        //         // 이전 Bgm 이어서 Play 되도록? - 기획과 협상
        //     }
        // }

        // private void FadeOut()
        // {
        //     fadeSec
        // }
    }
}
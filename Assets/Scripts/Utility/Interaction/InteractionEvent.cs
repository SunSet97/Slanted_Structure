using System;
using System.Collections.Generic;
using System.Linq;
using Move;
using Play;
using UnityEngine;
using UnityEngine.Events;
using Utility.Property;

namespace Utility.Interaction
{
    [Serializable]
    public class InteractionEvents
    {
        [SerializeField]
        public List<InteractionEvent> interactionEvents;

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
                    InteractEvent();
                    break;
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

        private void InteractEvent()
        {
            foreach (var t in interactObjs.interacts)
            {
                var interaction = t.interactObj.GetInteraction(t.index);
                interaction.isInteractable = t.interactable;
            }
        }
    }
}
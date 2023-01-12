using System;
using Move;
using Play;
using UnityEngine;

[Serializable]
public class InteractionEvents
{
    [SerializeField]
    public InteractionEvent[] interactionEvents;
}
[Serializable]
public class InteractionEvent
{
    public enum EventType { None, Clear, Active, Move, Play, Interactable }
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
        public InteractionObj_stroke interactObj;
        public bool interactable;
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
            // t.gameObject.GetComponent<IPlayable>().IsPlay = t.isPlay;
        }
    }

    private void InteractEvent()
    {
        foreach (var t in interactObjs.interacts)
        {
            t.interactObj.enabled = t.interactable;
        }
    }
}
using System;
using Move;
using Play;
using UnityEngine;

[Serializable]
public class InteractionEventMedium
{
    [SerializeField]
    public InteractionEvent[] interactionEvents;
}
[Serializable]
public class InteractionEvent
{
    public enum EventType { NONE, CLEAR, ACTIVE, MOVE, PLAY }
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
    [ConditionalHideInInspector("eventType", EventType.CLEAR)]
    public CheckMapClear clearBox;

    [ConditionalHideInInspector("eventType", EventType.ACTIVE)]
    public Active activeObjs;

    [ConditionalHideInInspector("eventType", EventType.MOVE)]
    public MovableList movables;
        
    [ConditionalHideInInspector("eventType", EventType.PLAY)]
    public PlayableList playableList;

    public void ClearEvent()
    {
        Debug.Log($"Clear Event - {clearBox.gameObject}");
        clearBox.Clear();
    }
    
    public void ActiveEvent()
    {
        foreach (var t in activeObjs.actives)
        {
            Debug.Log($"ACTIVE Event - {t.activeObject}: {t.activeSelf}");
            t.activeObject.SetActive(t.activeSelf);
        }
    }

    public void MoveEvent()
    {
        foreach (MovableObj t in movables.movables)
        {
            Debug.Log($"Move Event - {t.gameObject}: {t.isMove}");
            t.gameObject.GetComponent<IMovable>().IsMove = t.isMove;
        }
    }

    public void PlayEvent()
    {
        foreach (var t in playableList.playableObjs)
        {
            Debug.Log($"Play Event - {t.gameObject}: {t.isPlay} ");
            t.gameObject.GetComponent<IPlayable>().Play();
            // t.gameObject.GetComponent<IPlayable>().IsPlay = t.isPlay;
        }
    }
}
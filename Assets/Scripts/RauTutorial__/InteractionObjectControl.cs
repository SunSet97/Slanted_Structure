using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MiniGameClass : MonoBehaviour
{ 
    public delegate void CallBack();
    public CallBack callBack;

    public virtual void on() {}
    public virtual void off() {}
}

public class InteractionClass : MonoBehaviour
{
    public delegate void CallBack();
    public CallBack callBack;

    public virtual void interact(){}
}

public enum InteractionType
{
    Click,
    Enter
}

public class InteractionObjectControl : MonoBehaviour
{
    public bool isInteractable;
    public InteractionClass interactionClass;
    public bool hasMiniGame;
    public MiniGameClass miniGame;
    public InteractionType interactionType;

    public UnityEvent onMiniGameEvent;
    public UnityEvent offMiniGameEvent;
    public UnityEvent EventCallBeforeInteraction;
    public UnityEvent EventCallAfterInteraction;
    
    private CanvasControl _canvasInstance;
    private Camera _cam;
    private Collider _col;

    public class sampleInteractionClass : InteractionClass
    {
        public override void interact()
        {
            callBack();
        }
    }
    
    private void Start()
    {
        if (miniGame != null)
            miniGame.callBack = offMiniGame;

        if (interactionClass == null)
        {
            var sic = this.gameObject.AddComponent<sampleInteractionClass>();
            interactionClass = sic;
        }

        if (interactionClass != null)
            interactionClass.callBack = () =>
            {
                offInteraction();
                if(miniGame != null)
                    onMiniGame();
            };
        
        _cam = Camera.main;
        isInteractable = true;

        if (interactionType == InteractionType.Enter)
        {
            _col = GetComponent<Collider>();
            if(_col == null)
                Debug.Log( this.gameObject.name + " Error : this Interaction Type needs collider");
        }
    }

    private void Update()
    {
        if (isInteractable) interaction();
    }

    public void interaction()
    {
        switch (interactionType)
        {
            case InteractionType.Click :
                clickInteract();
                break;
            case InteractionType.Enter :
                //enterInteraction();
                break;
        }
    }

    private void clickInteract()
    {
        if (Input.GetMouseButtonDown(0) == false)
            return;
        
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray,out hit))
        {
            if (hit.collider.gameObject == this.gameObject)
            {
                callEvent();
            }
        }
    }

    private void onInteraction()
    {
        isInteractable = false;
        EventCallBeforeInteraction.Invoke();
    }

    private void offInteraction()
    {
        EventCallAfterInteraction.Invoke();
        isInteractable = true;
    }

    private void callEvent()
    {
        onInteraction();
        interactionClass.interact();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (interactionType != InteractionType.Enter)
            return;

        callEvent();
    }

    private void onMiniGame()
    {
        isInteractable = false;
        
        if(onMiniGameEvent != null)
            onMiniGameEvent.Invoke();
        
        miniGame.on();
    }

    private void offMiniGame()
    {
        miniGame.off();
        
        if(offMiniGameEvent != null)
            offMiniGameEvent.Invoke();
        
        EventCallAfterInteraction.Invoke();
        
        isInteractable = true;
    }
}

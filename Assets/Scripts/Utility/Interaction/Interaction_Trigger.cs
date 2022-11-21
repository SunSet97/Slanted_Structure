﻿using Data;
using UnityEditor.Recorder.Input;
using UnityEngine;

public class Interaction_Trigger : InteractionObj_stroke
{
    private bool isActivated;

    protected override void OnTriggerEnter(Collider other)
    {
        if (isActivated || interactionMethod != CustomEnum.InteractionMethod.Trigger)
        {
            return;
        }
        
        Debug.Log(gameObject.name + "트리거  " + other.transform.gameObject + isActivated);
        isActivated = true;
        StartInteraction();
    }
}

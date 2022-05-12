using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Data;
using UnityEngine;

public class Interaction_Trigger : InteractionObj_stroke
{
    public bool isLoop;

    protected override void Start()
    {
        base.Start();
        interactionMethod = CustomEnum.InteractionMethod.No;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isLoop) return;
        isLoop = true;
        StartInteraction();
    }
}

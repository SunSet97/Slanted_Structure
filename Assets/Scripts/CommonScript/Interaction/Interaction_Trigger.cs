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
        Debug.Log("트리거");
        if (isLoop) return;
        isLoop = true;
        StartInteraction();
    }
}

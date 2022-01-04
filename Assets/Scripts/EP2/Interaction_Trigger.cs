using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction_Trigger : InteractionObj_stroke
{
    public bool isLoop;

    void OnTriggerEnter(Collider other)
    {
        interactionResponse();
        gameObject.SetActive(isLoop);
    }
}

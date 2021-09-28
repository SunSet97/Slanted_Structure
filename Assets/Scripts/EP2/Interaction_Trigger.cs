using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction_Trigger : MonoBehaviour
{
    private bool isStart = false;
    void OnTriggerEnter(Collider other)
    {
        if (!isStart && TryGetComponent(out InteractionObj_stroke interactionObj_Stroke))
        {
            interactionObj_Stroke.interactionResponse();
            isStart = true;
        }
    }
}

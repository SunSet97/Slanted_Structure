using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction_Trigger : MonoBehaviour
{   
    void OnTriggerEnter(Collider other)
    {
        if (TryGetComponent(out InteractionObj_stroke interactionObj_Stroke))
        {
            interactionObj_Stroke.interactionResponse();
            gameObject.SetActive(false);
        }
    }
}

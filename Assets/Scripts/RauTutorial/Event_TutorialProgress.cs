using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event_TutorialProgress : MonoBehaviour
{
    public RauTutorialManager rauTutorialManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Rau") rauTutorialManager.isCheckPoint[rauTutorialManager.checkPoint.FindIndex(point => point == this.transform)] = true;
    }
}

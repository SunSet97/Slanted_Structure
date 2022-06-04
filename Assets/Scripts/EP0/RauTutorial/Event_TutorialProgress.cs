using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event_TutorialProgress : MonoBehaviour
{
    public RauTutorialManager rauTutorialManager;

    void OnTriggerEnter(Collider other)
    {
        rauTutorialManager.Play(rauTutorialManager.checkPoint.FindIndex(point => point == transform));
        rauTutorialManager.isCheckPoint[rauTutorialManager.checkPoint.FindIndex(point => point == transform)] = true;
    }
}

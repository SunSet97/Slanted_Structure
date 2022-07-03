using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;

public class Event_TutorialProgress : MonoBehaviour
{
    public RauTutorialManager rauTutorialManager;

    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out CharacterManager speat)) return;
        if (speat != DataController.instance.GetCharacter(CustomEnum.Character.Main)) return;
        
        var transIdx = rauTutorialManager.checkPoint.FindIndex(point => point == transform);
        rauTutorialManager.Play(transIdx);
        rauTutorialManager.isCheckPoint[transIdx] = true;
    }
}

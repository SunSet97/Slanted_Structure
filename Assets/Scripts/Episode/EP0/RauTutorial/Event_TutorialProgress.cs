using Data;
using UnityEngine;
using Utility.Core;

public class Event_TutorialProgress : MonoBehaviour
{
    public RauTutorialManager rauTutorialManager;

    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out CharacterManager character) &&
            character != DataController.Instance.GetCharacter(CustomEnum.Character.Main))
        {
            return;
        }

        var transIdx = rauTutorialManager.checkPoint.FindIndex(point => point == transform);
        rauTutorialManager.Play(transIdx);
    }
}

using System;
using UnityEngine;
using Utility.Character;
using Utility.Core;

namespace Episode.EP0.RauTutorial
{
    public class EventTutorialProgress : MonoBehaviour
    {
        [SerializeField] private RauTutorialManager rauTutorialManager;

        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out CharacterManager character) &&
                character != DataController.Instance.GetCharacter(CharacterType.Main))
            {
                return;
            }

            var checkedIndex = Array.FindIndex(rauTutorialManager.checkPoints, item => item.checkPoint == transform);
            rauTutorialManager.Play(checkedIndex);
        }
    }
}
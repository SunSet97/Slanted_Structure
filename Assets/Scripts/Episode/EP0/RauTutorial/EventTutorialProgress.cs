using System;
using UnityEngine;
using Utility.Character;
using Utility.Core;

namespace Episode.EP0.RauTutorial
{
    public class EventTutorialProgress : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private RauTutorialManager rauTutorialManager;
#pragma warning restore 0649
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
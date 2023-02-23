using System;
using CommonScript;
using Data;
using UnityEngine;
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
                character != DataController.Instance.GetCharacter(CustomEnum.Character.Main))
            {
                return;
            }

            var checkedIndex = Array.FindIndex(rauTutorialManager.checkPoints, item => item.checkPoint == transform);
            rauTutorialManager.Play(checkedIndex);
        }
    }
}
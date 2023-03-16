using Data.GamePlay;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Episode.EP2.PlatformGame
{
    public class PlatformGameManager : Game
    {
        [FormerlySerializedAs("InteractableButton")] [SerializeField]
        private Button interactableButton;

        public void ActiveButton(bool isActive, UnityAction unityAction = default)
        {
            interactableButton.gameObject.SetActive(isActive);
            if (unityAction == default)
            {
                interactableButton.onClick.RemoveAllListeners();
            }
            else
            {
                interactableButton.onClick.AddListener(unityAction);
            }
        }
    }
}


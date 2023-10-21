using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utility.Game;

namespace Episode.EP2.PlatformGame
{
    public class PlatformMiniGameManager : MiniGame
    {
        [SerializeField] private Button interactionButton;

        public void ActiveButton(bool isActive, UnityAction unityAction = default)
        {
            interactionButton.gameObject.SetActive(isActive);
            if (unityAction == default)
            {
                interactionButton.onClick.RemoveAllListeners();
            }
            else
            {
                interactionButton.onClick.AddListener(unityAction);
            }
        }
    }
}
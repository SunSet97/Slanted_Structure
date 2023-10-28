using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utility.Game;

namespace Episode.EP2.PlatformGame
{
    public class PlatformMiniGameManager : MiniGame
    {
#pragma warning disable 0649
        [SerializeField] private Button interactionButton;
#pragma warning restore 0649
        
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
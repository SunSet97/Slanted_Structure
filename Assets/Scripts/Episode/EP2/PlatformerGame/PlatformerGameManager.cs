using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlatformerGameManager : MonoBehaviour
{
    public enum ButtonType
    {
        jumpObstacle, jumpFence, jumpWall, slideWall, jumpWallClimbing
    }

    [SerializeField]
    private Button InteractableButton;

    public void ActiveButton(bool isActive, UnityAction unityAction)
    {   
        InteractableButton.gameObject.SetActive(isActive);
        InteractableButton.onClick.AddListener(unityAction);
    }

    public void ActiveButton(bool isActive)
    {
        InteractableButton.gameObject.SetActive(isActive);
        InteractableButton.onClick.RemoveAllListeners();
    }
}


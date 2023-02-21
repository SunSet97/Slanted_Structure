using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlatformerGameManager : MonoBehaviour
{
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


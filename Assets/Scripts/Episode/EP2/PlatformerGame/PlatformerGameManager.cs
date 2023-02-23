using Play;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlatformerGameManager : MonoBehaviour, IGamePlayable
{
    [SerializeField]
    private Button InteractableButton;

    public bool IsPlay { get; set; }

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

    public void EndPlay()
    {
        IsPlay = false;
    }

    public void Play()
    {
        IsPlay = true;
    }
}


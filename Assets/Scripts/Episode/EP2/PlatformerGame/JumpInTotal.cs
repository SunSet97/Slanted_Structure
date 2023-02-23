using UnityEngine;

public abstract class JumpInTotal : MonoBehaviour
{
    public PlatformerGameManager gameManager;
        
    protected bool isActivated;

    protected virtual void OnTriggerEnter(Collider other) {
        gameManager.ActiveButton(true, ButtonPressed);
    }
    protected virtual void OnTriggerExit(Collider other)
    {
        gameManager.ActiveButton(false);
    }
    protected abstract void ButtonPressed();
}

using UnityEngine;

public abstract class JumpInTotal : MonoBehaviour
{
    public PlatformerGameManager gameManager;
    public PlatformerGameManager.ButtonType btnType;

    protected bool isActivated;

    protected virtual void OnTriggerEnter(Collider other) {
        //버튼의 범위가 겹칠때는?
        gameManager.ActiveButton(true, ButtonPressed);
    }
    protected virtual void OnTriggerExit(Collider other)
    {
        gameManager.ActiveButton(false);
    }
    protected abstract void ButtonPressed();
}

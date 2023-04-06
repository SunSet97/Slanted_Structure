using UnityEngine;

namespace Episode.EP2.PlatformGame
{
    public abstract class Platform : MonoBehaviour
    {
        [SerializeField] protected PlatformMiniGameManager miniGameManager;

        protected virtual void OnTriggerEnter(Collider other)
        {
            miniGameManager.ActiveButton(true, PressButton);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            miniGameManager.ActiveButton(false);
        }

        protected abstract void PressButton();
    }
}
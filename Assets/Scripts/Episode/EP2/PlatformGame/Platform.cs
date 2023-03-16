using UnityEngine;

namespace Episode.EP2.PlatformGame
{
    public abstract class Platform : MonoBehaviour
    {
        public PlatformGameManager gameManager;

        protected virtual void OnTriggerEnter(Collider other) {
            gameManager.ActiveButton(true, PressButton);
        }
        protected virtual void OnTriggerExit(Collider other)
        {
            gameManager.ActiveButton(false);
        }
        protected abstract void PressButton();
    }
}
